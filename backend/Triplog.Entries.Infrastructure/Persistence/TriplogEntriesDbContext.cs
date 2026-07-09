using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;

namespace Triplog.Entries.Infrastructure.Persistence;

public sealed class TriplogEntriesDbContext(DbContextOptions<TriplogEntriesDbContext> options)
    : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Entry> Entries => Set<Entry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TriplogEntriesDbContext).Assembly);
    }
}