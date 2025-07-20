using System;

namespace rinha_backend_csharp.Dtos;

public record PaymentProcessorRequest(
    Guid CorrelationId,
    decimal Amount,
    DateTime RequestedAt
);