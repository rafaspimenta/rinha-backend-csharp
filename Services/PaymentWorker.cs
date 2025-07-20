using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using rinha_backend_csharp.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentWorker(
    IPaymentQueue paymentQueue,
    PaymentService paymentService,
    IConfiguration configuration,
    ILogger<PaymentWorker> logger)
    : BackgroundService
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")!;
    
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => ProcessPaymentAsync(token), token));
        }

        await Task.WhenAll(tasks);
    }

    private async Task ProcessPaymentAsync(CancellationToken token)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);

        while (await paymentQueue.Reader.WaitToReadAsync(token))
        {
            while (paymentQueue.Reader.TryRead(out var paymentRequest))
            {
                try
                {
                    await paymentService.ProcessPaymentAsync(paymentRequest, connection, token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing payment for correlation {CorrelationId}",
                        paymentRequest.CorrelationId);
                }
            }
        }
    }
}