using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Triplog.Entries.Domain.Common;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");

        builder.HasKey(t => t.Id);

        // Strongly typed IDs → Guid columns
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TripId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.OwnerId)
            .HasConversion(id => id.Value, value => new OwnerId(value))
            .IsRequired();

        // TripTitle wraps a string — flatten to a single column via HasConversion
        builder.Property(t => t.Title)
            .HasConversion(title => title.Value, value => TripTitle.Create(value).Value)
            .HasMaxLength(TripTitle.MaxLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        // DateRange has two fields — use ComplexProperty to keep the value object as-is
        // Columns become: dates_start_date, dates_end_date (snake_case naming plugin handles it)
        builder.ComplexProperty(t => t.Dates, dates =>
        {
            dates.Property(d => d.StartDate).IsRequired();
            dates.Property(d => d.EndDate).IsRequired();
        });

        // Enum → string (per ADR — string persistence is reorder-safe)
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.ArchivedAtUtc);

        // DomainEvents are transient — never persisted
        builder.Ignore(t => t.DomainEvents);

        // Query performance — most reads filter by owner
        builder.HasIndex(t => t.OwnerId);
    }
}