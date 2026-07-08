using Microsoft.EntityFrameworkCore;

namespace Triplog.Entries.Infrastructure;

public sealed class TriplogEntriesDbContext(DbContextOptions<TriplogEntriesDbContext> options)
    : DbContext(options)
{
    // DbSets and OnModelCreating land in Phase 4.
    // Kept minimal here so the project compiles green before we add configurations.
}