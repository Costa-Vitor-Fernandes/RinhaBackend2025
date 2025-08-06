using System.Text.Json;
using RinhaBackend2025.Clients;
using RinhaBackend2025.Models;
using RinhaBackend2025.Repositories;

namespace RinhaBackend2025.Services;

public class PaymentsService
{
    private readonly RedisRepository _redisRepository;
    private readonly PaymentsProcessorClient _paymentsProcessorClient;

    private const string DefaultProcessorUrl = "http://payment-processor-default:8080";
    private const string FallbackProcessorUrl = "http://payment-processor-fallback:8080";

    public PaymentsService(RedisRepository redisRepository, PaymentsProcessorClient paymentsProcessorClient)
    {
        _redisRepository = redisRepository;
        _paymentsProcessorClient = paymentsProcessorClient;
    }

    /// <summary>
    /// Processa um pagamento, escolhendo o processador mais rápido e registrando o pagamento no repositório.
    /// </summary>
    public async Task ProcessPaymentAsync(PostPaymentRequestDto paymentRequest)
    {
        // var defaultHealth = await GetOrCreateHealthCheckAsync(DefaultProcessorUrl, "default");
        // var fallbackHealth = await GetOrCreateHealthCheckAsync(FallbackProcessorUrl, "fallback");
        var defaultHealthTask = GetOrCreateHealthCheckAsync(DefaultProcessorUrl, "default");
        var fallbackHealthTask = GetOrCreateHealthCheckAsync(FallbackProcessorUrl, "fallback");

        await Task.WhenAll(defaultHealthTask, fallbackHealthTask);

        //priorizando o menor tempo de resposta
        var defaultHealth = await defaultHealthTask;
        var fallbackHealth = await fallbackHealthTask;
        var healthyProcessors = new List<(string url, string type, int responseTime)>();

        if (!defaultHealth.Failing)
        {
            healthyProcessors.Add((DefaultProcessorUrl, "default", defaultHealth.MinResponseTime));
        }

        if (!fallbackHealth.Failing)
        {
            healthyProcessors.Add((FallbackProcessorUrl, "fallback", fallbackHealth.MinResponseTime));
        }

        string chosenProcessorUrl;
        string chosenProcessorType;

        if (healthyProcessors.Any())
        {
            // Escolhe o processador com o menor MinResponseTime entre os saudáveis
            var bestProcessor = healthyProcessors.OrderBy(p => p.responseTime).First();
            chosenProcessorUrl = bestProcessor.url;
            chosenProcessorType = bestProcessor.type;
        }
        else
        {
            // Fallback para o processador padrão se ambos estiverem falhando
            chosenProcessorUrl = DefaultProcessorUrl;
            chosenProcessorType = "default";
        }

        // Cria a requisição completa para o Payment Processor, adicionando o requestedAt
        var paymentProcessorRequest = new ProcessorPaymentRequestDto
        {
            CorrelationId = paymentRequest.CorrelationId,
            Amount = paymentRequest.Amount,
            RequestedAt = DateTime.UtcNow // Adiciona o timestamp atual em UTC
        };

        try
        {
            await _paymentsProcessorClient.ProcessPaymentAsync(chosenProcessorUrl, paymentProcessorRequest);

            // Salva o pagamento individualmente no repositório
            await _redisRepository.SavePaymentAsync(chosenProcessorType, paymentRequest, paymentProcessorRequest.RequestedAt);
        }
        catch (HttpRequestException)
        {
            // O pagamento falhou, e nenhuma alteração será feita no resumo.
        }
    }

    /// <summary>
    /// Obtém o resumo dos pagamentos, filtrando por um período opcional.
    /// </summary>
    public async Task<PaymentsSummaryDto> GetPaymentsSummaryAsync(DateTime? from, DateTime? to)
    {
        // Se a requisição não tem filtros de data, use o método mais rápido
        if (from == null && to == null)
        {
            return await _redisRepository.GetTotalPaymentsSummaryAsync();
        }
        // Se houver filtros, use o método que filtra pelo Sorted Set
        else
        {
            return await _redisRepository.GetPaymentsSummaryByDateAsync(from, to);
        }
    }

    private async Task<ServiceHealthDto> GetOrCreateHealthCheckAsync(string url, string type)
    {
        var cachedHealthCheck = await _redisRepository.GetCachedHealthCheckAsync(type);

        if (!string.IsNullOrEmpty(cachedHealthCheck))
        {
            return JsonSerializer.Deserialize<ServiceHealthDto>(cachedHealthCheck, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        int maxRetries = 5;
        int retryDelayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var healthCheck = await _paymentsProcessorClient.GetHealthCheckAsync(url);
                var jsonHealthCheck = JsonSerializer.Serialize(healthCheck);

                await _redisRepository.SetCachedHealthCheckAsync(type, jsonHealthCheck, TimeSpan.FromSeconds(4.5));
                return healthCheck;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao obter health check para {url} (tentativa {i + 1}/{maxRetries}): {ex.Message}");
                if (i < maxRetries - 1)
                {
                    await Task.Delay(retryDelayMs * (i + 1));
                }
                else
                {
                    return new ServiceHealthDto { Failing = true, MinResponseTime = int.MaxValue };
                }
            }
        }
        return new ServiceHealthDto { Failing = true, MinResponseTime = int.MaxValue };
    }
}
