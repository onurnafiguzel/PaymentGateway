namespace PaymentOrchestrator.Application.ReadModels.Payments.Dto;

public sealed record PaymentTimelineEventDto(
    string Type,
    string Description,
    DateTime CreatedAtUtc
);
