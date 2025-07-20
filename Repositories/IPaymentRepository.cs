using System;
using System.Threading;
using System.Threading.Tasks;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Repositories;

public interface IPaymentRepository
{
    Task SavePaymentAsync(Payment payment, CancellationToken token);
    Task<PaymentSummaryResponse> GetSummaryAsync(DateTime? start, DateTime? end, CancellationToken token);
    Task PurgePaymentsAsync(CancellationToken token);
} 