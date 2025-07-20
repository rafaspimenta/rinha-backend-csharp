using System.Threading.Tasks;

namespace rinha_backend_csharp.Services.Health;

public interface IHealthCheck
{
    Task<bool> IsHealthyAsync();
} 