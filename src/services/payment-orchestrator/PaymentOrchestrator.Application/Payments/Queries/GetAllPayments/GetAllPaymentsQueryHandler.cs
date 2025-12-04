using MediatR;
using PaymentOrchestrator.Application.Abstractions;
using PaymentOrchestrator.Application.Payments.Dto;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Queries.GetAllPayments;

public sealed class GetAllPaymentsQueryHandler(
    IPaymentRepository paymentRepository
) : IRequestHandler<GetAllPaymentsQuery, Result<List<PaymentDto>>>
{
    public async Task<Result<List<PaymentDto>>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetAllAsync(cancellationToken);

        if (payments.Count == 0)
            return Result<List<PaymentDto>>.Success([]);

        var list = payments.Select(p => new PaymentDto(
            p.Id,
            p.MerchantId,
            p.Amount,
            p.Currency,
            p.Status.ToString(),
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();

        return Result<List<PaymentDto>>.Success(list);
    }
}
