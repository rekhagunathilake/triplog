using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Domain.Entries;
using Triplog.Entries.Domain.Trips;
using Triplog.Entries.Infrastructure.Persistence.Sagas;

namespace Triplog.Entries.Infrastructure.Persistence;

public sealed class TriplogEntriesDbContext(DbContextOptions<TriplogEntriesDbContext> options)
    : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<PublishEntrySagaState> PublishEntrySagaStates => Set<PublishEntrySagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TriplogEntriesDbContext).Assembly);
    }
}