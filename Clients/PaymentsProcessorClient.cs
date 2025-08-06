using System.Text;
using System.Text.Json;
using RinhaBackend2025.Models;

namespace RinhaBackend2025.Clients;

public class PaymentsProcessorClient
{
    private readonly HttpClient _httpClient;

    public PaymentsProcessorClient(HttpClient httpClient) // AQUI ESTÁ A CORREÇÃO!
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceHealthDto> GetHealthCheckAsync(string baseUrl)
    {
        var url = $"{baseUrl}/payments/service-health";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new ServiceHealthDto { Failing = true, MinResponseTime = int.MaxValue };
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ServiceHealthDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task ProcessPaymentAsync(string baseUrl, ProcessorPaymentRequestDto paymentRequest)
    {
        var url = $"{baseUrl}/payments";

        var json = JsonSerializer.Serialize(paymentRequest); // Serializa o objeto completo
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
}
