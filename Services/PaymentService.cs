using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;
using rinha_backend_csharp.Queue;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Services.PaymentProcessor;

namespace rinha_backend_csharp.Services;

public class PaymentService(
    IEnumerable<IPaymentProcessorStrategy> processors,
    IPaymentRepository repository,
    IPaymentQueue queue,
    ILogger<PaymentService> logger)
{
    public async Task ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken token)
    {
        var requestedAt = DateTime.UtcNow;
        var processorRequest = new PaymentProcessorRequest(
            paymentRequest.CorrelationId,
            paymentRequest.Amount,
            requestedAt);

        foreach (var processor in processors)
        {
            if (!await processor.CanProcessAsync())
            {
                continue;
            }

            if (!await processor.ProcessAsync(processorRequest, token))
            {
                continue;
            }
            
            var payment = new Payment
            {
                CorrelationId = paymentRequest.CorrelationId,
                Amount = paymentRequest.Amount,
                RequestedAt = requestedAt,
                ProcessorType = processor.ProcessorType
            };

            try
            {
                await repository.SavePaymentAsync(payment, token);
                logger.LogInformation(
                    "Payment processed successfully with {ProcessorType}", 
                    processor.ProcessorType);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, 
                    "Error saving payment for correlation {CorrelationId}",
                    paymentRequest.CorrelationId);
            }
        }

        logger.LogWarning(
            "All processors failed, re-queueing payment {CorrelationId}", 
            paymentRequest.CorrelationId);
        await Task.Delay(100, token);
        await queue.EnqueueAsync(paymentRequest);
    }
}