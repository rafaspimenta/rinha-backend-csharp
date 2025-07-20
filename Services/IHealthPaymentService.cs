using System.Threading.Tasks;

namespace rinha_backend_csharp.Services;

public interface IHealthPaymentService
{
    Task<bool> IsDefaultOnlineAsync();
    Task<bool> IsFallbackOnlineAsync();
} 