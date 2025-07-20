using System.Threading;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public interface IPaymentProcessorClient
{
    Task<bool> ProcessPaymentAsync(PaymentProcessorRequest request, string processorUrl, CancellationToken token);
} 