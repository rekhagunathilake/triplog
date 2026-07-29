using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Triplog.Entries.Infrastructure.Persistence.Sagas;

namespace Triplog.Entries.Infrastructure.Persistence.Configurations;

public sealed class PublishEntrySagaStateConfiguration : IEntityTypeConfiguration<PublishEntrySagaState>
{
    public void Configure(EntityTypeBuilder<PublishEntrySagaState> builder)
    {
        builder.ToTable("publish_entry_saga_state");
        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FirstFailureReason).HasMaxLength(2000);

        // MassTransit uses Version for optimistic concurrency
        builder.Property(x => x.Version).IsConcurrencyToken();

        // Index for queuing saga instance by current state
        builder.HasIndex(x => x.CurrentState);
    }
}
