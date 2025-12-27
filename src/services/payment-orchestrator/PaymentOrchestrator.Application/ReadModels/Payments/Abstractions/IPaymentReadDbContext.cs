using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;

namespace PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;

public interface IPaymentReadDbContext
{
    DbSet<PaymentTimeline> PaymentTimelines { get; }
    DbSet<PaymentTimelineEvent> PaymentTimelineEvents { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
