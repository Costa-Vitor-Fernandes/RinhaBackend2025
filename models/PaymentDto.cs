namespace RinhaBackend2025.Models;

// Representa a requisição POST /payments
public class PaymentRequestDto
{
    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
}

// Representa o resumo dos pagamentos para o GET /payments-summary
public class PaymentsSummaryDto
{
    public ProcessorSummary Default { get; set; } = new ProcessorSummary();
    public ProcessorSummary Fallback { get; set; } = new ProcessorSummary();
}

public class ProcessorSummary
{
    public long TotalRequests { get; set; }
    public decimal TotalAmount { get; set; }
}
public class ServiceHealthDto
{
    public bool Failing { get; set; }
    public int MinResponseTime { get; set; }
}
