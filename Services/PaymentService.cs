using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;
using rinha_backend_csharp.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentService(
    IHealthPaymentService healthPaymentService,
    IPaymentQueue paymentQueue,
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentProcessorSettings> processorSettings,
    ILogger<PaymentService> logger)
{
    private readonly PaymentProcessorSettings _processorSettings = processorSettings.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    
    private const string PaymentInsertSql = """
                       INSERT INTO payments (correlation_id, amount, requested_at, processor_type)
                       VALUES (@CorrelationId, @Amount, @RequestedAt, @ProcessorType)
                       """;

    public async Task ProcessPaymentAsync(PaymentRequest paymentRequest, NpgsqlConnection connection, CancellationToken token)
    {
        var defaultUsed = false;
        var fallbackUsed = false;
        var requestedAt = DateTime.UtcNow;
        var processorType = ProcessorType.Default;

        var processorPaymentRequest = new PaymentProcessorRequest(
            paymentRequest.CorrelationId,
            paymentRequest.Amount,
            requestedAt);
        
        if (await healthPaymentService.IsDefaultOnlineAsync())
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync(
                    _processorSettings.DefaultUrl + "/payments", 
                    processorPaymentRequest,
                    AppJsonSerializerContext.Default.PaymentProcessorRequest, 
                    token);
                
                defaultUsed = result.IsSuccessStatusCode;
                logger.LogInformation("Payment processed with Default processor used: {processorUsed}", defaultUsed);
            }
            catch (Exception)
            {
                logger.LogWarning("Default processor is not available, using fallback: {processorUsed}", defaultUsed);
            }
        }
        
        if (!defaultUsed && await healthPaymentService.IsFallbackOnlineAsync())
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync(
                    _processorSettings.FallbackUrl + "/payments", 
                    processorPaymentRequest,
                    AppJsonSerializerContext.Default.PaymentProcessorRequest,
                    token);
                
                fallbackUsed = result.IsSuccessStatusCode;
                processorType = ProcessorType.Fallback;
                logger.LogInformation("Payment processed with Fallback processor used: {processorUsed}", fallbackUsed);
            }
            catch (Exception)
            {
                logger.LogWarning("Fallback processor is not available, re-queueing payment {paymentRequest}", paymentRequest);
            }
        }
            
        if (!defaultUsed && !fallbackUsed)
        {
            logger.LogWarning("Default and fallback processors are not available, using fallback");
            await Task.Delay(100, token);
            await paymentQueue.EnqueueAsync(paymentRequest);
            return;
        }
            
        var payment = new Payment
        {
            CorrelationId = paymentRequest.CorrelationId,
            Amount = paymentRequest.Amount,
            RequestedAt = requestedAt,
            ProcessorType = processorType
        };

        try
        {
            await SavePaymentAsync(payment, connection, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving payment for correlation {CorrelationId}",
                paymentRequest.CorrelationId);
            await Task.Delay(100, token);
            await paymentQueue.EnqueueAsync(paymentRequest);
        }
    }

    private static async Task SavePaymentAsync(Payment payment, NpgsqlConnection connection, CancellationToken token)
    {
        await using var cmd = new NpgsqlCommand(PaymentInsertSql, connection);
        cmd.Parameters.Add("CorrelationId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = payment.CorrelationId;
        cmd.Parameters.Add("Amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = payment.Amount;
        cmd.Parameters.Add("RequestedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz).Value = payment.RequestedAt;
        cmd.Parameters.Add("ProcessorType", NpgsqlTypes.NpgsqlDbType.Varchar).Value = payment.ProcessorType.ToString();

        var success = await cmd.ExecuteNonQueryAsync(token);
        if (success != 1)
        {
            throw new Exception("Error saving payment");
        }
    }
}