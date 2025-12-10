using MediatR;
using MassTransit;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint
) : IRequestHandler<CreatePaymentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        // 1) DDD factory
        var createResult = Payment.Create(
            request.MerchantId,
            request.Amount,
            request.Currency
        );

        if (createResult.IsFailure)
            return Result<int>.Failure(createResult.Error);

        var payment = createResult.Value!;

        // 2) DB add
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3) Publish payment created (MassTransit style)
        var @event = new PaymentCreatedEvent(
            payment.Id,
            payment.MerchantId,
            payment.Amount,
            payment.Currency,
            payment.CreatedAt
        );

        await publishEndpoint.Publish(@event, cancellationToken);

        // 4) return id
        return Result<int>.Success(payment.Id);
    }
}
