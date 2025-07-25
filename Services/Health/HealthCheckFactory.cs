using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Configs;

namespace rinha_backend_csharp.Services.Health;

public class HealthCheckFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentProcessorSettings> settings)
{
    private readonly PaymentProcessorSettings _settings = settings.Value;
    private const string HealthEndpoint = "/payments/service-health";

    public ITimeBasedHealthCheck CreateDefaultProcessor()
    {
        var healthCheck = new HttpEndpointHealthCheck(
            httpClientFactory,
            _settings.DefaultUrl + HealthEndpoint);
        return new TimeBasedHealthCheck(
            healthCheck, 
            TimeSpan.FromMilliseconds(_settings.HealthCheckIntervalMilliseconds),
            TimeSpan.FromMilliseconds(_settings.FaultHealthCheckIntervalMilliseconds));
    }

    public ITimeBasedHealthCheck CreateFallbackProcessor()
    {
        var healthCheck = new HttpEndpointHealthCheck(
            httpClientFactory,
            _settings.FallbackUrl + HealthEndpoint);
        return new TimeBasedHealthCheck(
            healthCheck, 
            TimeSpan.FromMilliseconds(_settings.HealthCheckIntervalMilliseconds),
            TimeSpan.FromMilliseconds(_settings.FaultHealthCheckIntervalMilliseconds));
    }
} 