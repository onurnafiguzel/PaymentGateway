using MediatR;
using PaymentOrchestrator.Application.ReadModels.Payments.Dto;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Queries;

public sealed record GetPaymentTimelineQuery(Guid PaymentId)
    : IRequest<PaymentTimelineDto>;
