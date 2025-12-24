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
) : IRequestHandler<CreatePaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
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
            return Result<Guid>.Failure(createResult.Error);

        var payment = createResult.Value!;

        // 2) DB add
        await paymentRepository.AddAsync(payment, cancellationToken);


        // 3) Publish event (EF Outbox'a yazılır, RabbitMQ'ya hemen gitmez)
       
        await publishEndpoint.Publish(new PaymentCreatedEvent
        {
            CorrelationId = payment.Id,
            PaymentId = payment.Id,
            MerchantId = payment.MerchantId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            CreatedAt = payment.CreatedAt
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(payment.Id);
    }
}
