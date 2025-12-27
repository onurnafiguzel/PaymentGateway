namespace PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;

public interface IPaymentTimelineProjectionWriter
{
    Task EnsureTimelineExistsAsync(Guid paymentId, CancellationToken ct);

    Task UpdateStatusAsync(Guid paymentId, string status, CancellationToken ct);

    Task AddEventAsync(
        Guid paymentId,
        string type,
        string description,
        CancellationToken ct);
}
