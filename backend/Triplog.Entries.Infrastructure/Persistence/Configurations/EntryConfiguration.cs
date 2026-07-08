using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Persistence.Configurations;

public sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.ToTable("entries");

        builder.HasKey(e => e.Id);

        // Strongly typed IDs → Guid columns
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new EntryId(value))
            .ValueGeneratedNever();

        builder.Property(e => e.TripId)
            .HasConversion(id => id.Value, value => new TripId(value))
            .IsRequired();

        builder.Property(e => e.OwnerId)
            .HasConversion(id => id.Value, value => new OwnerId(value))
            .IsRequired();

        // Single-field VOs → primitive columns
        builder.Property(e => e.Title)
            .HasConversion(title => title.Value, value => EntryTitle.Create(value).Value)
            .HasMaxLength(EntryTitle.MaxLength)
            .IsRequired();

        builder.Property(e => e.Body)
            .HasConversion(body => body.Value, value => EntryBody.Create(value).Value)
            .HasMaxLength(EntryBody.MaxLength)
            .IsRequired();

        builder.Property(e => e.VisitedOn).IsRequired();

        // Location is optional (Location?) — OwnsOne handles nullability cleanly
        // When Location is null on the aggregate, all three columns are NULL in the DB
        builder.OwnsOne(e => e.Location, location =>
        {
            location.Property(l => l.Name)
                .HasMaxLength(Location.MaxLocationNameLength)
                .IsRequired();
            location.Property(l => l.Latitude).IsRequired();
            location.Property(l => l.Longitude).IsRequired();
        });

        // Enum → string
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.PublishedAtUtc);
        builder.Property(e => e.ArchivedAtUtc);

        builder.Property(e => e.LastPublishFailReason).HasMaxLength(2000);

        // MediaReferences collection → separate entry_media_references table
        builder.OwnsMany(e => e.MediaReferences, media =>
        {
            media.ToTable("entry_media_references");

            // Owned entities need an FK back to the owner + their own PK
            media.WithOwner().HasForeignKey("entry_id");
            media.Property<int>("id").ValueGeneratedOnAdd();
            media.HasKey("id");

            media.Property(m => m.Id)
                .HasConversion(id => id.Value, value => new MediaReferenceId(value))
                .IsRequired();

            media.Property(m => m.DisplayOrder).IsRequired();

            // Enforce the domain invariant "no duplicate media per entry" at the DB level too
            media.HasIndex("entry_id", nameof(MediaReference.Id)).IsUnique();
        });

        // Point EF Core at the private _mediaReferences backing field so it doesn't try
        // to write through the read-only IReadOnlyCollection property
        builder.Navigation(e => e.MediaReferences)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // DomainEvents are transient
        builder.Ignore(e => e.DomainEvents);

        // Indexes for common query paths
        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => e.TripId);
    }
}