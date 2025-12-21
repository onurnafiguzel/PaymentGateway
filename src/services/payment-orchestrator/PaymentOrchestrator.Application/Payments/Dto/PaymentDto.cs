namespace PaymentOrchestrator.Application.Payments.Dto;

public sealed record PaymentDto(
    Guid Id,
    string MerchantId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
