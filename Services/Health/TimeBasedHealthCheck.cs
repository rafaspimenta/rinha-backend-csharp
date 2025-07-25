using System;
using System.Threading;
using System.Threading.Tasks;

namespace rinha_backend_csharp.Services.Health;

public class TimeBasedHealthCheck(IHealthCheck innerHealthCheck, TimeSpan cacheDuration, TimeSpan? faultCacheDuration = null) : ITimeBasedHealthCheck
{
    private readonly SemaphoreSlim _lock = new(1);
    
    private bool _lastPaymentStatus;

    public TimeSpan CacheDuration { get; } = cacheDuration;
    public TimeSpan FaultCacheDuration { get; } = faultCacheDuration ?? TimeSpan.FromSeconds(1);
    public DateTime LastCheckTime { get; private set; } = DateTime.MinValue;

    public bool LastStatus { get; private set; }
    
    public async Task<bool> IsHealthyAsync()
    {
        var now = DateTime.UtcNow;
        
        // Fast path: If last payment succeeded, assume system is healthy
        if (_lastPaymentStatus)
        {
            return true;
        }
        
        // Determine cache duration based on payment status
        var effectiveCacheDuration = GetEffectiveCacheDuration();
        
        // Return cached result if not expired
        if (IsCacheValid(now, effectiveCacheDuration))
        {
            return LastStatus;
        }

        // Cache expired, perform actual health check
        return await PerformHealthCheckAsync(now);
    }
    
    private TimeSpan GetEffectiveCacheDuration()
    {
        // Use shorter cache duration when payments are failing (more frequent checks)
        // Use normal cache duration when payments are succeeding (less frequent checks)
        return _lastPaymentStatus ? CacheDuration : FaultCacheDuration;
    }
    
    private bool IsCacheValid(DateTime now, TimeSpan cacheDuration)
    {
        return now - LastCheckTime < cacheDuration;
    }
    
    private async Task<bool> PerformHealthCheckAsync(DateTime now)
    {
        await _lock.WaitAsync();
        try
        {
            // Double-check cache validity after acquiring lock
            if (IsCacheValid(now, GetEffectiveCacheDuration()))
            {
                return LastStatus;
            }
            
                // Perform actual health check
                LastStatus = await innerHealthCheck.IsHealthyAsync();
                _lastPaymentStatus = LastStatus;
                LastCheckTime = now;
            
            return LastStatus;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    /// <summary>
    /// Updates the health check cache based on payment processing results.
    /// This allows the health check to adapt its caching strategy:
    /// - Successful payments indicate system health (longer cache duration)
    /// - Failed payments indicate potential issues (shorter cache duration)
    /// </summary>
    /// <param name="paymentSucceeded">Whether the last payment attempt succeeded</param>
    public void UpdateFromPaymentResult(bool paymentSucceeded)
    {
        var now = DateTime.UtcNow;
        _lastPaymentStatus = paymentSucceeded;
        LastStatus = paymentSucceeded;
        LastCheckTime = now;
    }
} 