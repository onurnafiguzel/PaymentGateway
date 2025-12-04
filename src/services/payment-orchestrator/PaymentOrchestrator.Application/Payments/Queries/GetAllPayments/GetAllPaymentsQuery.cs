using MediatR;
using PaymentOrchestrator.Application.Payments.Dto;
using PaymentOrchestrator.Domain.Payments;
using Shared.Kernel.Domain.Results;

namespace PaymentOrchestrator.Application.Payments.Queries.GetAllPayments;

public sealed record GetAllPaymentsQuery()
    : IRequest<Result<List<PaymentDto>>>;