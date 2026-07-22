using Microsoft.EntityFrameworkCore;
using Triplog.Media.Domain.MediaItems;

namespace Triplog.Media.Infrastructure;

public sealed class TriplogMediaDbContext(DbContextOptions<TriplogMediaDbContext> options)
: DbContext(options)
{
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TriplogMediaDbContext).Assembly);
    }
}
