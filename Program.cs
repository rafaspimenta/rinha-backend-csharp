using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Services;
using rinha_backend_csharp.Services.PaymentProcessor;
using rinha_backend_csharp.Services.Queue;

var builder = WebApplication.CreateSlimBuilder(args);

// Configure Kestrel for high-volume requests
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxConcurrentConnections = 2000;
    options.Limits.MaxConcurrentUpgradedConnections = 2000;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(120);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.AllowSynchronousIO = false; // Keep async-only for better performance

    options.Limits.MinRequestBodyDataRate = null;
    options.Limits.MinResponseDataRate = null;
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

builder.Services.AddPaymentProcessorHttpClient();
builder.Services.AddSingleton<IPaymentQueue, PaymentQueue>();
builder.Services.AddSingleton<IPaymentProcessorClient, HttpPaymentProcessorClient>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IPaymentProcessorStrategy, DefaultPaymentProcessorStrategy>();
builder.Services.AddSingleton<IPaymentProcessorStrategy, FallbackPaymentProcessorStrategy>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddHostedService<PaymentWorker>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapPost("/payments", async (
    [FromBody] PaymentRequest request,
    [FromServices] IPaymentQueue queue) =>
{
    await queue.EnqueueAsync(request);
    return Results.Accepted();
}).DisableAntiforgery();

app.MapGet("/payments-summary", async (
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromServices] IPaymentRepository repository,
    CancellationToken token) =>
{
    var response = await repository.GetSummaryAsync(from, to, token);
    return Results.Ok(response);
}).DisableAntiforgery();

app.MapPost("/purge-payments", async (
    [FromServices] IPaymentRepository repository,
    CancellationToken token) =>
{
    await repository.PurgePaymentsAsync(token);
    return Results.Ok(new PaymentPurgeResponse("All payments purged permanently."));
}).DisableAntiforgery();

app.Run();