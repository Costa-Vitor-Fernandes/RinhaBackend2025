namespace RinhaBackend2025.Models;

public class PostPaymentRequestDto
{
    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
}

// DTO para a requisição que seu backend envia para o Payment Processor
public class ProcessorPaymentRequestDto
{
    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
}

// Representa o resumo dos pagamentos para o GET /payments/summary
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
