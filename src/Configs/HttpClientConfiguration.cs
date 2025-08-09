using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace rinha_backend_csharp.Configs;

public static class HttpClientConfiguration
{
    public static void AddPaymentProcessorHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("PaymentProcessor", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<PaymentProcessorSettings>>().Value;
            client.Timeout = TimeSpan.FromMilliseconds(settings.HttpClientTimeoutMilliseconds);
        })
        .AddPolicyHandler((serviceProvider, _) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<PaymentProcessorSettings>>().Value;

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    settings.RetryCount,
                    _ =>
                        TimeSpan.FromMilliseconds(settings.RetryJitterMilliseconds) +
                        TimeSpan.FromMilliseconds(new Random().Next(0, settings.RetryJitterMilliseconds)));
            return retryPolicy;
        });
    }
} 