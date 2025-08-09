using System;

namespace rinha_backend_csharp.Dtos;

public record PaymentRequest (
    Guid CorrelationId,
    decimal Amount);
