using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Dto;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Queries;

public sealed class GetPaymentTimelineQueryHandler
    : IRequestHandler<GetPaymentTimelineQuery, PaymentTimelineDto?>
{
    private readonly IPaymentReadDbContext _db;

    public GetPaymentTimelineQueryHandler(IPaymentReadDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentTimelineDto?> Handle(
        GetPaymentTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var timeline = await _db.PaymentTimelines
            .Include(x => x.Events)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PaymentId == request.PaymentId,
                cancellationToken);

        if (timeline is null)
            return null;

        return new PaymentTimelineDto(
            timeline.PaymentId,
            timeline.CurrentStatus,
            timeline.Events
                .OrderBy(x => x.CreatedAtUtc)
                .Select(e => new PaymentTimelineEventDto(
                    e.Type,
                    e.Description,
                    e.CreatedAtUtc))
                .ToList());
    }
}
