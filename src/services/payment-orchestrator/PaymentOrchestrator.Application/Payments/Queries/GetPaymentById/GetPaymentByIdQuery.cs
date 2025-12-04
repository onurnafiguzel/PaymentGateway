using MediatR;
using PaymentOrchestrator.Application.Payments.Dto;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(int Id)
    : IRequest<Result<PaymentDto>>;
