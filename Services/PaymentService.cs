using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using rinha_backend_csharp.Dtos;
using rinha_backend_csharp.Repositories;
using rinha_backend_csharp.Repositories.Models;
using rinha_backend_csharp.Services.PaymentProcessor;
using rinha_backend_csharp.Services.Queue;

namespace rinha_backend_csharp.Services;

public class PaymentService(
    IEnumerable<IPaymentProcessorStrategy> processors,
    IPaymentRepository repository,
    IPaymentQueue queue,
    ILogger<PaymentService> logger)
{
    public async Task ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken token)
    {
        var requestedAt = DateTime.UtcNow;
        var processorRequest = new PaymentProcessorRequest(
            paymentRequest.CorrelationId,
            paymentRequest.Amount,
            requestedAt);

        foreach (var processor in processors)
        {
            if (!await processor.CanProcessAsync())
            {
                continue;
            }

            if (!await processor.ProcessAsync(processorRequest, token))
            {
                continue;
            }

            var payment = new Payment
            {
                CorrelationId = paymentRequest.CorrelationId,
                Amount = paymentRequest.Amount,
                RequestedAt = requestedAt,
                ProcessorType = processor.ProcessorType
            };

            await repository.SavePaymentAsync(payment, token);
            return;
        }

        logger.LogWarning(
            "All processors failed, re-queueing payment {CorrelationId}",
            paymentRequest.CorrelationId);
        await queue.EnqueueAsync(paymentRequest);
    }
}