using System.Threading;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public interface IPaymentProcessorStrategy
{
    ProcessorType ProcessorType { get; }
    Task<bool> CanProcessAsync();
    Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token);
} 