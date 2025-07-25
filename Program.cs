using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Queue;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Services;
using rinha_backend_csharp.Services.Health;
using rinha_backend_csharp.Services.PaymentProcessor;

ThreadPool.SetMinThreads(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);

var builder = WebApplication.CreateSlimBuilder(args);

// Configure Kestrel for high-volume requests
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    options.Limits.MaxRequestBodySize = 1024; // 1KB - payments are small
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.AllowSynchronousIO = false; // Keep async-only for better performance
});

// Disable features not needed for high-performance API
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = false; // Skip URL transformation
    options.LowercaseQueryStrings = false;
    options.AppendTrailingSlash = false;
});

// Optimize logging for production (minimize allocations)
builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.None;
});

builder.Services.Configure<PaymentProcessorSettings>(builder.Configuration.GetSection("PaymentProcessor"));

// Configure HTTP client with timeout
builder.Services.AddHttpClient("PaymentProcessor", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<PaymentProcessorSettings>>().Value;
    client.Timeout = TimeSpan.FromMilliseconds(settings.HttpClientTimeoutMilliseconds);
});

// Configure dedicated HTTP client for health checks with shorter timeout
builder.Services.AddHttpClient("HealthCheck", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<PaymentProcessorSettings>>().Value;
    // Use a shorter timeout for health checks to make them more responsive
    client.Timeout = TimeSpan.FromMilliseconds(settings.HealthCheckTimeoutMilliseconds);
});

builder.Services.AddSingleton<IPaymentQueue, PaymentQueue>();
builder.Services.AddSingleton<IPaymentProcessorClient, HttpPaymentProcessorClient>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IPaymentProcessorStrategy, DefaultPaymentProcessorStrategy>();
builder.Services.AddSingleton<IPaymentProcessorStrategy, FallbackPaymentProcessorStrategy>();
builder.Services.AddSingleton<PaymentService>();

// Configure health checks
builder.Services.AddSingleton<HealthCheckFactory>();
builder.Services.AddSingleton<IHealthPaymentProcessorService>(sp =>
{
    var factory = sp.GetRequiredService<HealthCheckFactory>();
    return new HealthPaymentProcessorService(
        factory.CreateDefaultProcessor(),
        factory.CreateFallbackProcessor());
});

builder.Services.AddHostedService<PaymentWorker>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapPost("/payments", async (PaymentRequest request, IPaymentQueue queue) =>
{
    await queue.EnqueueAsync(request);
    return Results.Accepted();
}).DisableAntiforgery();

app.MapGet("/payments-summary", async (
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    IPaymentRepository repository,
    CancellationToken token) =>
{
    var response = await repository.GetSummaryAsync(from, to, token);
    return Results.Ok(response);
}).DisableAntiforgery();

app.MapPost("/purge-payments", async (
    IPaymentRepository repository,
    CancellationToken token) =>
{
    await repository.PurgePaymentsAsync(token);
    return Results.Ok(new PaymentPurgeResponse("All payments purged permanently."));
}).DisableAntiforgery();

app.Run();