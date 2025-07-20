using System;
using System.Threading;
using System.Threading.Tasks;

namespace rinha_backend_csharp.Services.Health;

public class TimeBasedHealthCheck(IHealthCheck innerHealthCheck, TimeSpan cacheDuration) : ITimeBasedHealthCheck
{
    private readonly SemaphoreSlim _lock = new(1);

    public TimeSpan CacheDuration { get; } = cacheDuration;
    public DateTime LastCheckTime { get; private set; } = DateTime.MinValue;

    public bool LastStatus { get; private set; }

    public async Task<bool> IsHealthyAsync()
    {
        if (DateTime.UtcNow - LastCheckTime < CacheDuration)
            return LastStatus;

        await _lock.WaitAsync();
        try
        {
            if (DateTime.UtcNow - LastCheckTime >= CacheDuration)
            {
                LastStatus = await innerHealthCheck.IsHealthyAsync();
                LastCheckTime = DateTime.UtcNow;
            }
        }
        finally
        {
            _lock.Release();
        }

        return LastStatus;
    }
} 