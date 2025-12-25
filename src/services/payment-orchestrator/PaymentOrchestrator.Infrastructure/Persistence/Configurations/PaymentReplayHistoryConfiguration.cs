using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Infrastructure.Persistence.Configurations;

public sealed class PaymentReplayHistoryConfiguration
    : IEntityTypeConfiguration<PaymentReplayHistory>
{
    public void Configure(EntityTypeBuilder<PaymentReplayHistory> builder)
    {
        builder.ToTable("payment_replay_histories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.TriggeredBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.PaymentId);
    }
}
