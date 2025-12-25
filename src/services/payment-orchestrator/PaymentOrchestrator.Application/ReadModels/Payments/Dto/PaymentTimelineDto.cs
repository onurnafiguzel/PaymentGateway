namespace PaymentOrchestrator.Application.ReadModels.Payments.Dto;

public sealed record PaymentTimelineDto(
    Guid PaymentId,
    string CurrentStatus,
    IReadOnlyList<PaymentTimelineEventDto> Events
);
