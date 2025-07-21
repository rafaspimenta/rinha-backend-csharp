using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace rinha_backend_csharp.Services.Health;

public class HttpEndpointHealthCheck(IHttpClientFactory httpClientFactory, string healthCheckUrl)
    : IHealthCheck
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(healthCheckUrl);
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.TooManyRequests;
        }
        catch
        {
            return false;
        }
    }
} 