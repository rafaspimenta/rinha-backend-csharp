using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;
using rinha_backend_csharp.Queue;
using rinha_backend_csharp.Services;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IPaymentQueue, PaymentQueue>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<IHealthPaymentService, HealthPaymentService>();
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

const string sqlSummary =
    """
    SELECT
        COUNT(*) FILTER (WHERE upper(processor_type) = 'DEFAULT') AS DefaultTotalRequests,
        COALESCE(SUM(amount) FILTER (WHERE upper(processor_type) = 'DEFAULT'), 0) AS DefaultTotalAmount,
        COUNT(*) FILTER (WHERE upper(processor_type) = 'FALLBACK') AS FallbackTotalRequests,
        COALESCE(SUM(amount) FILTER (WHERE upper(processor_type) = 'FALLBACK'), 0) AS FallbackTotalAmount
    FROM payments
    WHERE requested_at BETWEEN @start AND @end;
    """;

const string sqlPurgePayments = "TRUNCATE payments;";

app.MapGet("/payments-summary", async (
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    IConfiguration config) =>
{
    var start = from ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    var end = to ?? DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

    await using var conn = new NpgsqlConnection(config.GetConnectionString("Postgres"));
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(sqlSummary, conn);
    cmd.Parameters.AddWithValue("start", NpgsqlTypes.NpgsqlDbType.TimestampTz, start);
    cmd.Parameters.AddWithValue("end", NpgsqlTypes.NpgsqlDbType.TimestampTz, end);

    await using var reader = await cmd.ExecuteReaderAsync();
    await reader.ReadAsync();

    var response = new PaymentSummaryResponse(
        new PaymentSummary(reader.GetInt32(0), reader.GetDecimal(1)),
        new PaymentSummary(reader.GetInt32(2), reader.GetDecimal(3))
    );

    return Results.Ok(response);
});

app.MapPost("/purge-payments", async (IConfiguration config) =>
{
    await using var conn = new NpgsqlConnection(config.GetConnectionString("Postgres"));
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(sqlPurgePayments, conn);
    await cmd.ExecuteNonQueryAsync();

    return Results.Ok(new PaymentPurgeResponse("All payments purged permanently."));
});

app.Run();