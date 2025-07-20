namespace rinha_backend_csharp.Configs;

public class PaymentProcessorSettings
{
    public string DefaultUrl { get; set; } = string.Empty;
    public string FallbackUrl { get; set; } = string.Empty;
}
