using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public class DefaultPaymentProcessorStrategy(
    IPaymentProcessorClient processorClient,
    IOptions<PaymentProcessorSettings> settings)
    : IPaymentProcessorStrategy
{
    private readonly string _processorUrl = settings.Value.DefaultUrl;

    public ProcessorType ProcessorType => ProcessorType.Default;

    public Task<bool> CanProcessAsync() => Task.FromResult(true); // future optimization checking the health endpoint 

    public async Task<bool> ProcessAsync(PaymentProcessorRequest request, CancellationToken token)
    {
        var success = await processorClient.ProcessPaymentAsync(request, _processorUrl, token);
        return success;
    }
} 