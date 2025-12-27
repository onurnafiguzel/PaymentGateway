using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.ReadModels.Common.Entities;
using PaymentOrchestrator.Application.ReadModels.Payments.Abstractions;
using PaymentOrchestrator.Application.ReadModels.Payments.Entities;

namespace PaymentOrchestrator.ReadModel.Persistence;

public sealed class PaymentReadDbContext : DbContext, IPaymentReadDbContext
{
    public PaymentReadDbContext(DbContextOptions<PaymentReadDbContext> options)
        : base(options) { }

    public DbSet<PaymentTimeline> PaymentTimelines => Set<PaymentTimeline>();
    public DbSet<PaymentTimelineEvent> PaymentTimelineEvents => Set<PaymentTimelineEvent>();
    public DbSet<ProcessedReadEvent> ProcessedReadEvents => Set<ProcessedReadEvent>();

    public new DbSet<TEntity> Set<TEntity>() where TEntity : class
      => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentTimeline>(b =>
        {
            b.ToTable("payment_timelines");
            b.HasKey(x => x.PaymentId);

            b.Property(x => x.CurrentStatus)
                .HasMaxLength(50)
                .IsRequired();

            b.HasMany(x => x.Events)
                .WithOne()
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentTimelineEvent>(b =>
        {
            b.ToTable("payment_timeline_events");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            b.HasIndex(x => x.PaymentId);
        });

        modelBuilder.Entity<ProcessedReadEvent>(b =>
        {
            b.ToTable("processed_read_events");

            b.HasKey(x => x.Id);

            b.Property(x => x.MessageId)
                .IsRequired();

            b.Property(x => x.ConsumerName)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.ProcessedAtUtc)
                .IsRequired();

            b.HasIndex(x => new { x.MessageId, x.ConsumerName })
                .IsUnique();
        });
    }
}
