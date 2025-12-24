using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Persistence;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdatePaymentStatusCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePaymentStatusCommand request,
        CancellationToken cancellationToken)
    {
        // 1) DB’den getir
        var payment = await paymentRepository.GetByIdAsync(
            request.PaymentId,
            cancellationToken
        );

        if (payment is null)
            return Result.Failure(
                new("payment.not_found", "Payment not found.")
            );

        // 2) Domain behavior çağrısı (DDD)
        var updateResult = payment.UpdateStatus(request.NewStatus);

        if (updateResult.IsFailure)
            return updateResult;

        // 3) Save
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
