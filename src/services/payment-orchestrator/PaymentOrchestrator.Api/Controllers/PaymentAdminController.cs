using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaymentOrchestrator.Application.Payments.Commands.ReplayPayment;
using PaymentOrchestrator.Application.Payments.Dto;
using PaymentOrchestrator.Application.ReadModels.Payments.Queries;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("api/admin/payments")]
public sealed class PaymentAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }
   
    [EnableRateLimiting("replay-policy")]
    [HttpPost("{paymentId:guid}/replay")]
    public async Task<IActionResult> Replay(
        Guid paymentId,
        [FromBody] ReplayPaymentRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ReplayPaymentCommand(paymentId, request.Reason),
            ct);

        if (result.IsFailure)
            return BadRequest(new
            {
                result.Error.Code,
                result.Error.Message
            });

        return Ok(new
        {
            PaymentId = paymentId,
            Status = "ReplayRequested"
        });
    }

    [HttpGet("api/admin/payments/{paymentId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid paymentId)
    {
        var result = await _mediator.Send(new GetPaymentTimelineQuery(paymentId));
        return result is null ? NotFound() : Ok(result);
    }
}
