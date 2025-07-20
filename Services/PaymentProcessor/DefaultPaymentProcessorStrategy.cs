using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public class DefaultPaymentProcessorStrategy(
    IHealthPaymentProcessorService healthService,
    IPaymentProcessorClient processorClient,
    IOptions<PaymentProcessorSettings> settings)
    : IPaymentProcessorStrategy
{
    private readonly string _processorUrl = settings.Value.DefaultUrl;

    public ProcessorType ProcessorType => ProcessorType.Default;

    public Task<bool> CanProcessAsync() => healthService.IsDefaultOnlineAsync();

    public Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token) =>
        processorClient.ProcessPaymentAsync(request, _processorUrl, token);
} 