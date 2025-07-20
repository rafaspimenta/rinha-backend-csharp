namespace rinha_backend_csharp.Dtos;

public record PaymentSummaryResponse(
    PaymentSummary Default,
    PaymentSummary Fallback
);

public record PaymentSummary(long TotalRequests, decimal TotalAmount);
