using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public class FallbackPaymentProcessorStrategy(
    IHealthPaymentProcessorService healthService,
    IPaymentProcessorClient processorClient,
    IOptions<PaymentProcessorSettings> settings)
    : IPaymentProcessorStrategy
{
    private readonly string _processorUrl = settings.Value.FallbackUrl;

    public ProcessorType ProcessorType => ProcessorType.Fallback;

    public Task<bool> CanProcessAsync() => healthService.IsFallbackOnlineAsync();

    public Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token) =>
        processorClient.ProcessPaymentAsync(request, _processorUrl, token);
} 