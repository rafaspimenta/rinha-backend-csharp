using System.Threading.Tasks;
using rinha_backend_csharp.Services.Health;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public class HealthPaymentProcessorService(
    ITimeBasedHealthCheck defaultProcessor,
    ITimeBasedHealthCheck fallbackProcessor)
    : IHealthPaymentProcessorService
{
    public Task<bool> IsDefaultOnlineAsync() => defaultProcessor.IsHealthyAsync();
    
    public Task<bool> IsFallbackOnlineAsync() => fallbackProcessor.IsHealthyAsync();
} 