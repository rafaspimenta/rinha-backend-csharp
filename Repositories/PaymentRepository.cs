using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Repositories;

public class PaymentRepository(IConfiguration configuration) : IPaymentRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")
                                                ?? throw new ArgumentNullException(nameof(configuration));

    private const string PaymentInsertSql = """
                                            INSERT INTO payments (correlation_id, amount, requested_at, processor_type)
                                            VALUES (@CorrelationId, @Amount, @RequestedAt, @ProcessorType)
                                            """;

    private const string PaymentSummarySql = """
                                             SELECT
                                                 COUNT(*) FILTER (WHERE upper(processor_type) = 'DEFAULT') AS DefaultTotalRequests,
                                                 COALESCE(SUM(amount) FILTER (WHERE upper(processor_type) = 'DEFAULT'), 0) AS DefaultTotalAmount,
                                                 COUNT(*) FILTER (WHERE upper(processor_type) = 'FALLBACK') AS FallbackTotalRequests,
                                                 COALESCE(SUM(amount) FILTER (WHERE upper(processor_type) = 'FALLBACK'), 0) AS FallbackTotalAmount
                                             FROM payments
                                             WHERE requested_at BETWEEN @start AND @end;
                                             """;

    private const string PaymentPurgeSql = "TRUNCATE payments;";

    public async Task SavePaymentAsync(Payment payment, CancellationToken token)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);

        await using var cmd = new NpgsqlCommand(PaymentInsertSql, connection);
        cmd.Parameters.Add("CorrelationId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = payment.CorrelationId;
        cmd.Parameters.Add("Amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = payment.Amount;
        cmd.Parameters.Add("RequestedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz).Value = payment.RequestedAt;
        cmd.Parameters.Add("ProcessorType", NpgsqlTypes.NpgsqlDbType.Varchar).Value = payment.ProcessorType.ToString();

        var success = await cmd.ExecuteNonQueryAsync(token);
        if (success != 1)
        {
            throw new Exception("Error saving payment");
        }
    }

    public async Task<PaymentSummaryResponse> GetSummaryAsync(DateTime? start, DateTime? end, CancellationToken token)
    {
        var startDate = start ?? DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        var endDate = end ?? DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);

        await using var cmd = new NpgsqlCommand(PaymentSummarySql, connection);
        cmd.Parameters.AddWithValue("start", NpgsqlTypes.NpgsqlDbType.TimestampTz, startDate);
        cmd.Parameters.AddWithValue("end", NpgsqlTypes.NpgsqlDbType.TimestampTz, endDate);

        await using var reader = await cmd.ExecuteReaderAsync(token);
        await reader.ReadAsync(token);

        return new PaymentSummaryResponse(
            new PaymentSummary(reader.GetInt32(0), reader.GetDecimal(1)),
            new PaymentSummary(reader.GetInt32(2), reader.GetDecimal(3))
        );
    }

    public async Task PurgePaymentsAsync(CancellationToken token)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);

        await using var cmd = new NpgsqlCommand(PaymentPurgeSql, connection);
        await cmd.ExecuteNonQueryAsync(token);
    }
}