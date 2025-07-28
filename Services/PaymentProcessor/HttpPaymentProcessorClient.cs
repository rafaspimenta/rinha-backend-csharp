using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using rinha_backend_csharp.Configs;
using rinha_backend_csharp.Dtos;

namespace rinha_backend_csharp.Services.PaymentProcessor;

public class HttpPaymentProcessorClient(
    IHttpClientFactory httpClientFactory)
    : IPaymentProcessorClient
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PaymentProcessor");

    public async Task<bool> ProcessPaymentAsync(PaymentProcessorRequest request, string processorUrl, CancellationToken token)
    {
        try
        {
            var result = await _httpClient.PostAsJsonAsync(
                processorUrl + "/payments",
                request,
                AppJsonSerializerContext.Default.PaymentProcessorRequest,
                token);

            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
} 