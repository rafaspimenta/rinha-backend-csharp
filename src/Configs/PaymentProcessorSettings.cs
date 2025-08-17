namespace rinha_backend_csharp.Configs;

public class PaymentProcessorSettings
{
    public string DefaultUrl { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = string.Empty;
    public int HttpClientTimeoutMilliseconds { get; set; } = 60_000;
    public int RetryCount { get; set; } = 10;
    public int RetryJitterMilliseconds { get; set; } = 1000;
    public int MaxConcurrentPayments { get; set; }
}