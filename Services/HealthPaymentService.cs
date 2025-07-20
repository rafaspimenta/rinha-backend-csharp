using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using rinha_backend_csharp.Models;

namespace rinha_backend_csharp.Services;

public class HealthPaymentService(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentProcessorSettings> processorSettings)
    : IHealthPaymentService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    private DateTime _defaultLastCheck = DateTime.MinValue;
    private DateTime _fallbackLastCheck = DateTime.MinValue;
    private bool _isFallbackOnline;
    private bool _isDefaultOnline;
    private readonly SemaphoreSlim _defaultLock = new(1);
    private readonly SemaphoreSlim _fallbackLock = new(1);

    public async Task<bool> IsDefaultOnlineAsync()
    {
        if ((DateTime.UtcNow - _defaultLastCheck).TotalSeconds < 5)
            return _isDefaultOnline;

        await _defaultLock.WaitAsync();
        try
        {
            if ((DateTime.UtcNow - _defaultLastCheck).TotalSeconds >= 5)
            {
                try
                {
                    var res = await _httpClient.GetAsync(processorSettings.Value.DefaultUrl + "/payments/service-health");
                    _isDefaultOnline = res.IsSuccessStatusCode;
                }
                catch
                {
                    _isDefaultOnline = false;
                }

                _defaultLastCheck = DateTime.UtcNow;
            }
        }
        finally
        {
            _defaultLock.Release();
        }

        return _isDefaultOnline;
    }
    
    public async Task<bool> IsFallbackOnlineAsync()
    {
        if ((DateTime.UtcNow - _fallbackLastCheck).TotalSeconds < 5)
            return _isFallbackOnline;

        await _fallbackLock.WaitAsync();
        try
        {
            if ((DateTime.UtcNow - _fallbackLastCheck).TotalSeconds >= 5)
            {
                try
                {
                    var res = await _httpClient.GetAsync(processorSettings.Value.FallbackUrl);
                    _isFallbackOnline = res.IsSuccessStatusCode;
                }
                catch
                {
                    _isFallbackOnline = false;
                }

                _fallbackLastCheck = DateTime.UtcNow;
            }
        }
        finally
        {
            _fallbackLock.Release();
        }

        return _isFallbackOnline;
    }
}