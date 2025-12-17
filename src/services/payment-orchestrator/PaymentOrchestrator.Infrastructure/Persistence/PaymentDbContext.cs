using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Sagas.Payment;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentState> PaymentStates => Set<PaymentState>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SAGA STATE TABLOSU
        modelBuilder.Entity<PaymentState>()
            .HasKey(x => x.CorrelationId);

        // Inbox/Outbox mechanism of masstranst.efcore
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Tüm IEntityTypeConfiguration<T> implementasyonlarını otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}

