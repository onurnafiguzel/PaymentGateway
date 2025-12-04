using MediatR;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(
    string MerchantId,
    decimal Amount,
    string Currency = "TRY"
) : IRequest<Result<int>>; // return Payment.Id
