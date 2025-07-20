using System.Threading.Tasks;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public interface IHealthPaymentProcessorService
{
    Task<bool> IsDefaultOnlineAsync();
    Task<bool> IsFallbackOnlineAsync();
} 