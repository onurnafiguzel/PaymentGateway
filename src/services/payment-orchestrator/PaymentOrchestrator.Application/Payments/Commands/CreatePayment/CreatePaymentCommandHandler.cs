using MassTransit;
using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork
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

        // 3) Publish event (EF Outbox'a yazılır, RabbitMQ'ya hemen gitmez)
        var correlationId = Guid.NewGuid();
        await publishEndpoint.Publish(new PaymentCreatedEvent
        {
            CorrelationId = correlationId,
            PaymentId = payment.Id,
            MerchantId = payment.MerchantId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            CreatedAt = payment.CreatedAt
        });

        // 4) Atomic commit (Payment + OutboxMessage)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(payment.Id);
    }
}
