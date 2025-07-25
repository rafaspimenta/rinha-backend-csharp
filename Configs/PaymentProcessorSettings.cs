namespace rinha_backend_csharp.Configs;

public class PaymentProcessorSettings
{
    public string DefaultUrl { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = string.Empty;
    public int WorkerPoolSize { get; set; } = 5;
    public int HealthCheckIntervalMilliseconds { get; set; } = 1000;
    public int FaultHealthCheckIntervalMilliseconds { get; set; } = 1000;
    public int HttpClientTimeoutMilliseconds { get; set; } = 1000;
    public int HealthCheckTimeoutMilliseconds { get; set; } = 2000;
}
