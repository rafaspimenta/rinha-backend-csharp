using System;

namespace rinha_backend_csharp.Services.Health;

public interface ITimeBasedHealthCheck : IHealthCheck
{
    TimeSpan CacheDuration { get; }
    TimeSpan FaultCacheDuration { get; }
    DateTime LastCheckTime { get; }
    bool LastStatus { get; }
    void UpdateFromPaymentResult(bool paymentSucceeded);
} 