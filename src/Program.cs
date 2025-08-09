using System;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Services;
using rinha_backend_csharp.Services.PaymentProcessor;
using rinha_backend_csharp.Services.Queue;

var builder = WebApplication.CreateSlimBuilder(args);

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
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Error);
}

var app = builder.Build();

app.MapPost("/payments", (
    [FromBody] PaymentRequest request,
    [FromServices] IPaymentQueue queue) =>
{
    _ = queue.EnqueueAsync(request);
    return Results.Accepted();
});

app.MapGet("/payments-summary", async (
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromServices] IPaymentRepository repository,
    CancellationToken token) =>
{
    var response = await repository.GetSummaryAsync(from, to, token);
    return Results.Ok(response);
});

app.MapPost("/purge-payments", async (
    [FromServices] IPaymentRepository repository,
    CancellationToken token) =>
{
    await repository.PurgePaymentsAsync(token);
    return Results.Ok(new PaymentPurgeResponse("All payments purged permanently."));
});

app.Run();