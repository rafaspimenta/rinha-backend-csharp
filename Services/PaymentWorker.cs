using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using rinha_backend_csharp.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentWorker(
    IPaymentQueue paymentQueue,
    PaymentService paymentService,
    ILogger<PaymentWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await ProcessPaymentAsync(token);
    }

    private async Task ProcessPaymentAsync(CancellationToken token)
    {
        await foreach (var paymentRequest in paymentQueue.Reader.ReadAllAsync(token))
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
        }
    }
}