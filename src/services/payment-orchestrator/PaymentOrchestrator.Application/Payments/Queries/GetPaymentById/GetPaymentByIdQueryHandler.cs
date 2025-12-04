using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Payments.Dto;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler(
    IPaymentRepository paymentRepository
) : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(
            request.Id, cancellationToken);

        if (payment is null)
            return Result<PaymentDto>.Failure(
                new("payment.not_found", "Payment not found.")
            );

        var dto = new PaymentDto(
            payment.Id,
            payment.MerchantId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.CreatedAt,
            payment.UpdatedAt
        );

        return Result<PaymentDto>.Success(dto);
    }
}
