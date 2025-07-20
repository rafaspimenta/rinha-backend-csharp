using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Models;
using rinha_backend_csharp.Services.Health;

namespace rinha_backend_csharp.Services;

public class HealthPaymentProcessorService(
    ITimeBasedHealthCheck defaultProcessor,
    ITimeBasedHealthCheck fallbackProcessor)
    : IHealthPaymentProcessorService
{
    public Task<bool> IsDefaultOnlineAsync() => defaultProcessor.IsHealthyAsync();
    
    public Task<bool> IsFallbackOnlineAsync() => fallbackProcessor.IsHealthyAsync();
} 