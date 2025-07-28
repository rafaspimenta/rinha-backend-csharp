using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentWorker(
    IPaymentQueue paymentQueue,
    PaymentService paymentService,
    IOptions<PaymentProcessorSettings> settings,
    ILogger<PaymentWorker> logger)
    : BackgroundService
{
    private readonly SemaphoreSlim _semaphore = new(
        settings.Value.MaxConcurrentPayments, 
        settings.Value.MaxConcurrentPayments);

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await foreach (var paymentRequest in paymentQueue.Reader.ReadAllAsync(token))
        {
            await _semaphore.WaitAsync(token);
            _ = ProcessPaymentAsync(paymentRequest, token);
        }
    }

    private async Task ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken token)
    {
        try
        {
            await paymentService.ProcessPaymentAsync(paymentRequest, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment for correlation {CorrelationId}",
                paymentRequest.CorrelationId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}