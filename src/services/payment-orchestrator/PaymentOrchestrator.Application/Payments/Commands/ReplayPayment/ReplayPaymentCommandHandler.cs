using MassTransit;
using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Kernel.Domain.Results;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.ReplayPayment;

public sealed class ReplayPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReplayPaymentCommand, Result>
{
    public async Task<Result> Handle(ReplayPaymentCommand request, CancellationToken ct)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, ct);
        if (payment is null)
            return Result.Failure(new("payment.not_found", "Payment not found."));

        var replayResult = payment.Replay(request.Reason);
        if (replayResult.IsFailure)
            return replayResult;

        // IMPORTANT: Saga’yı yeniden başlatmak için aynı payment bilgileriyle PaymentCreated publish ediyoruz.
        await publishEndpoint.Publish(new PaymentCreatedEvent
        {
            CorrelationId = payment.Id,
            PaymentId = payment.Id,
            MerchantId = payment.MerchantId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            CreatedAt = payment.CreatedAt
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

//PaymentFailed
//   ↓
//ReplayPaymentRequestedEvent
//   ↓
//ReplayPaymentRequestedConsumer
//   ↓
//ReplayPaymentCommandHandler
//   ↓
//payment.Replay()
//   ↓
//PaymentCreatedEvent(same PaymentId)
//   ↓
//Saga yeniden başlar
