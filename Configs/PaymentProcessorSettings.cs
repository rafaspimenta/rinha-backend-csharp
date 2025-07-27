namespace rinha_backend_csharp.Configs;

public class PaymentProcessorSettings
{
    public string DefaultUrl { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = string.Empty;
    public int WorkerPoolSize { get; set; } = 5;
    public int HttpClientTimeoutMilliseconds { get; set; } = 60_000;
    public int RetryCount { get; set; } = 3;
    public int RetryJitterMilliseconds { get; set; } = 1000;
}
