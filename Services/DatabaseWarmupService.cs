using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace rinha_backend_csharp.Services;

public class DatabaseWarmupService(IConfiguration configuration) : IHostedService
{
    private readonly string _connectionString = configuration.GetConnectionString("Postgres")!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Warm up the database connection pool by establishing an initial connection
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        // Execute a simple query to verify database connectivity
        await using var cmd = new NpgsqlCommand("SELECT 1", connection);
        await cmd.ExecuteScalarAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
