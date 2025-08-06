using System.Text;
using System.Text.Json;
using RinhaBackend2025.Models;

namespace RinhaBackend2025.Clients;

public class PaymentsProcessorClient
{
    private readonly HttpClient _httpClient;

    public PaymentsProcessorClient()
    {
        _httpClient = new HttpClient();
    }
    public async Task<ServiceHealthDto> GetHealthCheckAsync(string baseUrl)
    {
        var url = $"{baseUrl}/payments/service-health";
        var response = await _httpClient.GetAsync(url);

        // Se a requisição não for bem-sucedida, assumimos que o serviço está falhando
        if (!response.IsSuccessStatusCode)
        {
            // Retorna um objeto indicando falha e tempo de resposta alto
            return new ServiceHealthDto { Failing = true, MinResponseTime = int.MaxValue };
        }

        var json = await response.Content.ReadAsStringAsync();

        // Deserializa a resposta JSON para o nosso modelo
        return JsonSerializer.Deserialize<ServiceHealthDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }


    public async Task ProcessPaymentAsync(string baseUrl, PaymentRequestDto paymentRequest)
    {
        // Cria a URL completa para o endpoint
        var url = $"{baseUrl}/payments";

        // Converte o objeto de pagamento para JSON
        var json = JsonSerializer.Serialize(new
        {
            correlationId = paymentRequest.CorrelationId,
            amount = paymentRequest.Amount,
            requestedAt = DateTime.UtcNow // O Payment Processor exige esse campo
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Faz a chamada HTTP POST para o Payment Processor
        var response = await _httpClient.PostAsync(url, content);

        // Lança uma exceção se a chamada não for bem sucedida (HTTP 5xx)
        response.EnsureSuccessStatusCode();
    }
}
