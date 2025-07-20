using System;

namespace rinha_backend_csharp.Models;

public class Payment
{
    public Guid CorrelationId { get; init; }
    public decimal Amount { get; init; }
    public DateTime RequestedAt { get; init; }
    public ProcessorType ProcessorType { get; init; }
}