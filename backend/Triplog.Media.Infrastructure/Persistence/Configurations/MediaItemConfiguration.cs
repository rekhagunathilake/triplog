using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Triplog.Media.Domain.Common;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Infrastructure.Persistence.Configurations;

public sealed class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.ToTable("media_items");

        builder.HasKey(m => m.Id);

        // Strongly-typed IDs → Guid
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => new MediaItemId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.OwnerId)
            .HasConversion(id => id.Value, value => new OwnerId(value))
            .IsRequired();

        // Single-field VOs
        builder.Property(m => m.BlobKey)
            .HasConversion(key => key.Value, value => BlobKey.Materialize(value))
            .IsRequired();

        builder.Property(m => m.ContentType)
            .HasConversion(ct => ct.Value, value => ContentType.Create(value).Value)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.SizeInBytes).IsRequired();
        builder.Property(m => m.OriginalFileName).HasMaxLength(255).IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.FinalizedAtUtc);
        builder.Property(m => m.FailedAtUtc);
        builder.Property(m => m.FailureReason).HasMaxLength(2000);

        builder.Ignore(m => m.DomainEvents);

        builder.HasIndex(m => m.OwnerId);
    }
}
