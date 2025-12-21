using MediatR;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Commands.UpdatePaymentStatus;

public record UpdatePaymentStatusCommand(
    Guid PaymentId,
    PaymentStatus NewStatus
) : IRequest<Result>;
