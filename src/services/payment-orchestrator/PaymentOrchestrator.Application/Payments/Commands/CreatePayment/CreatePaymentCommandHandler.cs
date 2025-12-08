using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;
using Shared.Messaging.Events.Common;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus
) : IRequestHandler<CreatePaymentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        // 1) DDD Factory çağrısı
        var createResult = Payment.Create(
            request.MerchantId,
            request.Amount,
            request.Currency
        );

        if (createResult.IsFailure)
            return Result<int>.Failure(createResult.Error);

        var payment = createResult.Value!;

        // 2) Repo → Add
        await paymentRepository.AddAsync(payment, cancellationToken);

        // 3) UoW → Commit
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 1️⃣ PaymentCreatedEvent publish
        var @event = new PaymentCreatedEvent(
            payment.Id,
            payment.MerchantId,
            payment.Amount,
            payment.Currency,
            payment.CreatedAt
        );

        await eventBus.PublishAsync(@event, cancellationToken);

        // 4) OK → Payment Id döneriz
        return Result<int>.Success(payment.Id);
    }
}
