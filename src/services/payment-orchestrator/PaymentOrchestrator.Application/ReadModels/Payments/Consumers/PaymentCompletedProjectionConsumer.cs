using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using Shared.Messaging.Events.Payments;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Consumers;

public sealed class PaymentCompletedProjectionConsumer
    : IConsumer<PaymentCompletedEvent>
{
    private readonly IPaymentReadDbContext _db;

    public PaymentCompletedProjectionConsumer(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var evt = context.Message;

        var timeline = await _db.PaymentTimelines
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.PaymentId == evt.PaymentId,
                context.CancellationToken);

        if (timeline is null)
            return;

        // Idempotency: aynı event iki kez gelirse tekrar yazma
        if (timeline.CurrentStatus == "Succeeded")
            return;

        timeline.UpdateStatus("Succeeded");
        timeline.AddEvent(
            "PaymentCompleted",
            $"Payment completed. PaymentId: {evt.PaymentId}");

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
