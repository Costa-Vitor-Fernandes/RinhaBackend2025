using Microsoft.AspNetCore.Mvc;
using RinhaBackend2025.Models;
using RinhaBackend2025.Services; // Vamos criar esse serviço depois
using RinhaBackend2025.Repositories;

namespace RinhaBackend2025.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsService _paymentsService;

    public PaymentsController(PaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost]
    public async Task<IActionResult> PostPayment([FromBody] PostPaymentRequestDto request)
    {
        // A lógica de roteamento e processamento será feita no serviço
        await _paymentsService.ProcessPaymentAsync(request);

        return Ok();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PaymentsSummaryDto>> GetPaymentsSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var summary = await _paymentsService.GetPaymentsSummaryAsync(from!, to!);
        return Ok(summary);

    }

}
