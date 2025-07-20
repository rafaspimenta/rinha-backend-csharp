using System;
using System.Collections.Generic;
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
        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => ProcessPaymentAsync(token), token));
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessPaymentAsync(CancellationToken token)
    {
        while (await paymentQueue.Reader.WaitToReadAsync(token))
        {
            while (paymentQueue.Reader.TryRead(out var paymentRequest))
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
}