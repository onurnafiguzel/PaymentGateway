using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging.Events.Fraud;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("api/test/fraud")]
public sealed class FraudTestController : ControllerBase
{
    private readonly IBus _bus;

    public FraudTestController(IBus bus)
    {
        _bus = bus;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteFraudCheck(
        [FromQuery] Guid correlationId,
        [FromQuery] Guid paymentId,
        [FromQuery] bool isFraud = false)
    {
        await _bus.Publish(new FraudCheckCompletedEvent(
            CorrelationId: correlationId,
            PaymentId: paymentId,
            IsFraud: isFraud,
            "fraud test",
            CheckedAt: DateTime.UtcNow
        ));

        return Ok(new
        {
            Message = "FraudCheckCompletedEvent published DIRECTLY",
            CorrelationId = correlationId,
            IsFraud = isFraud
        });
    }
}
