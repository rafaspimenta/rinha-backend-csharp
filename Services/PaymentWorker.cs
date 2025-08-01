using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Services.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentWorker(
    IPaymentQueue paymentQueue,
    PaymentService paymentService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await foreach (var paymentRequest in paymentQueue.Reader.ReadAllAsync(token))
        {
            await ProcessPaymentAsync(paymentRequest, token);
        }
    }

    private async Task ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken token)
    {
        await paymentService.ProcessPaymentAsync(paymentRequest, token);
    }
}