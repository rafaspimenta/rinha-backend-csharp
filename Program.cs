using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;
using rinha_backend_csharp.Queue;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Services;
using rinha_backend_csharp.Services.Health;
using rinha_backend_csharp.Services.PaymentProcessor;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient();
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

builder.Services.Configure<PaymentProcessorSettings>(builder.Configuration.GetSection("PaymentProcessor"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapPost("/payments", async (PaymentRequest request, IPaymentQueue queue) =>
{
    await queue.EnqueueAsync(request);
    return Results.Accepted();
});

app.MapGet("/payments-summary", async (
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    IPaymentRepository repository,
    CancellationToken token) =>
{
    var response = await repository.GetSummaryAsync(from, to, token);
    return Results.Ok(response);
});

app.MapPost("/purge-payments", async (
    IPaymentRepository repository,
    CancellationToken token) =>
{
    await repository.PurgePaymentsAsync(token);
    return Results.Ok(new PaymentPurgeResponse("All payments purged permanently."));
});

app.Run();