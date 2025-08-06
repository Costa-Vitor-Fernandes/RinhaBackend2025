using StackExchange.Redis;
using RinhaBackend2025.Models;
using System.Text.Json;

namespace RinhaBackend2025.Repositories;

public class RedisRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    // Métodos para cache do health-check
    public async Task<string?> GetCachedHealthCheckAsync(string processorType)
    {
        return await _db.StringGetAsync($"healthcheck:{processorType}");
    }

    public async Task SetCachedHealthCheckAsync(string processorType, string value, TimeSpan expiry)
    {
        await _db.StringSetAsync($"healthcheck:{processorType}", value, expiry);
    }

    // --- Novos métodos para Pagamentos ---

    /// <summary>
    /// Salva um pagamento processado em um Sorted Set.
    /// O score do Sorted Set é o timestamp, permitindo consultas por faixa de tempo.
    /// </summary>
    public async Task SavePaymentAsync(string processorType, PostPaymentRequestDto payment, DateTime requestedAt)
    {
        // Converte o DateTime para timestamp em milissegundos
        var timestamp = new DateTimeOffset(requestedAt).ToUnixTimeMilliseconds();

        // Salva o pagamento no Sorted Set para permitir a busca por tempo
        var key = $"payments:{processorType}";
        await _db.SortedSetAddAsync(key, payment.CorrelationId.ToString(), timestamp);

        // Salva os detalhes do pagamento em uma Hash para consulta futura
        var paymentDetailsKey = $"payment:details:{payment.CorrelationId}";
        var hashEntries = new HashEntry[]
        {
            new HashEntry("amount", payment.Amount.ToString()),
            new HashEntry("requestedAt", requestedAt.ToString("o")),
        };
        await _db.HashSetAsync(paymentDetailsKey, hashEntries);

        // Atualiza os contadores totais para o resumo sem filtro
        await _db.HashIncrementAsync($"summary:{processorType}", "totalAmount", (double)payment.Amount);
        await _db.HashIncrementAsync($"summary:{processorType}", "totalRequests", 1);
    }

    /// <summary>
    /// Obtém o resumo total dos pagamentos de forma otimizada (sem filtro).
    /// </summary>
    public async Task<PaymentsSummaryDto> GetTotalPaymentsSummaryAsync()
    {
        var defaultSummary = await _db.HashGetAllAsync("summary:default");
        var fallbackSummary = await _db.HashGetAllAsync("summary:fallback");

        static (long requests, decimal amount) ParseSummary(HashEntry[] entries)
        {
            long totalRequests = 0;
            decimal totalAmount = 0;
            foreach (var entry in entries)
            {
                if (entry.Name == "totalRequests" && long.TryParse(entry.Value, out var requests))
                    totalRequests = requests;
                if (entry.Name == "totalAmount" && decimal.TryParse(entry.Value, out var amount))
                    totalAmount = amount;
            }
            return (totalRequests, totalAmount);
        }

        var (defaultRequests, defaultAmount) = ParseSummary(defaultSummary);
        var (fallbackRequests, fallbackAmount) = ParseSummary(fallbackSummary);

        return new PaymentsSummaryDto
        {
            Default = new ProcessorSummary { TotalRequests = defaultRequests, TotalAmount = defaultAmount },
            Fallback = new ProcessorSummary { TotalRequests = fallbackRequests, TotalAmount = fallbackAmount }
        };
    }

    /// <summary>
    /// Obtém o resumo dos pagamentos para um período específico, usando Sorted Set.
    /// </summary>
    public async Task<PaymentsSummaryDto> GetPaymentsSummaryByDateAsync(DateTime? from, DateTime? to)
    {
        var defaultSummary = await GetProcessorSummaryWithDateFilterAsync("default", from, to);
        var fallbackSummary = await GetProcessorSummaryWithDateFilterAsync("fallback", from, to);

        return new PaymentsSummaryDto
        {
            Default = defaultSummary,
            Fallback = fallbackSummary
        };
    }

    private async Task<ProcessorSummary> GetProcessorSummaryWithDateFilterAsync(string processorType, DateTime? from, DateTime? to)
    {
        var key = $"payments:{processorType}";

        var min = from.HasValue ? new DateTimeOffset(from.Value).ToUnixTimeMilliseconds() : double.NegativeInfinity;
        var max = to.HasValue ? new DateTimeOffset(to.Value).ToUnixTimeMilliseconds() : double.PositiveInfinity;

        var paymentIds = await _db.SortedSetRangeByScoreAsync(key, min, max);

        if (paymentIds.Length == 0)
        {
            return new ProcessorSummary { TotalRequests = 0, TotalAmount = 0 };
        }

        var tasks = paymentIds.Select(id => _db.HashGetAllAsync($"payment:details:{id}")).ToList();
        var allDetails = await Task.WhenAll(tasks);

        long totalRequests = 0;
        decimal totalAmount = 0;

        foreach (var details in allDetails)
        {
            if (details.Length > 0)
            {
                var amountEntry = details.FirstOrDefault(e => e.Name == "amount");
                if (!amountEntry.Value.IsNullOrEmpty && decimal.TryParse(amountEntry.Value, out var amount))
                {
                    totalAmount += amount;
                    totalRequests++;
                }
            }
        }

        return new ProcessorSummary
        {
            TotalRequests = totalRequests,
            TotalAmount = totalAmount
        };
    }
}
