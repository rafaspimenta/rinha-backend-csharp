namespace rinha_backend_csharp.Models;

public class SummaryQueryResult
{
    public int DefaultTotalRequests { get; init; }
    public decimal DefaultTotalAmount { get; init; }
    public int FallbackTotalRequests { get; init; }
    public decimal FallbackTotalAmount { get; init; }
}