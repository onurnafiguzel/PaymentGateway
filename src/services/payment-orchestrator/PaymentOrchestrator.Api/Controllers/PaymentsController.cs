using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Api.Contracts.Payments;
using PaymentOrchestrator.Application.Payments.Commands.CreatePayment;
using PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;
using PaymentOrchestrator.Application.Payments.Queries.GetAllPayments;
using PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    // GET: api/payments
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPaymentsQuery(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    // GET: api/payments/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Code == "payment.not_found")
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    // POST: api/payments
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand(
            request.MerchantId,
            request.Amount,
            request.Currency
        );

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return result.IsSuccess
          ? Ok(result.Value)
          : BadRequest(new { error = result.Error });
    }

    // PUT: api/payments/{id}/status
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        Guid id,
        [FromBody] UpdatePaymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePaymentStatusCommand(id, request.Status);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Code == "payment.not_found")
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
