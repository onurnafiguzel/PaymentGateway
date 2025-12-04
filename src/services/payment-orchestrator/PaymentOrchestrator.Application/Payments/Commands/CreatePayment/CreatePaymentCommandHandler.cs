using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork
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

        // 4) OK → Payment Id döneriz
        return Result<int>.Success(payment.Id);
    }
}
