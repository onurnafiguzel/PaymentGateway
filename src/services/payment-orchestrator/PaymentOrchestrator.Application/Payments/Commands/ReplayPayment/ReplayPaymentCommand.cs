
using MediatR;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Commands.ReplayPayment;
public sealed record ReplayPaymentCommand(
    Guid PaymentId,
    string Reason
) : IRequest<Result>;
