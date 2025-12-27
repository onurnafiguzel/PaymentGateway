using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;
using PaymentOrchestrator.ReadModel.Persistence;

namespace PaymentOrchestrator.Infrastructure.ReadModels;

public sealed class PaymentTimelineProjectionWriter
    : IPaymentTimelineProjectionWriter
{
    private readonly PaymentReadDbContext _db;

    public PaymentTimelineProjectionWriter(PaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task EnsureTimelineExistsAsync(Guid paymentId, CancellationToken ct)
    {
        var exists = await _db.PaymentTimelines
            .AnyAsync(x => x.PaymentId == paymentId, ct);

        if (!exists)
        {
            _db.PaymentTimelines.Add(new PaymentTimeline(paymentId));
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateStatusAsync(Guid paymentId, string status, CancellationToken ct)
    {
        await _db.PaymentTimelines
            .Where(x => x.PaymentId == paymentId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.CurrentStatus, status),
                ct);
    }

    public async Task AddEventAsync(
        Guid paymentId,
        string type,
        string description,
        CancellationToken ct)
    {
        _db.PaymentTimelineEvents.Add(
            new PaymentTimelineEvent(paymentId, type, description));

        await _db.SaveChangesAsync(ct);
    }
}
