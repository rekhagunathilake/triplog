using Triplog.Entries.Application.Abstractions;

namespace Triplog.Entries.Infrastructure.Persistence;

public sealed class UnitOfWork(TriplogEntriesDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct);
}
