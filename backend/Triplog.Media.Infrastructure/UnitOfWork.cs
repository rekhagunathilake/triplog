using Triplog.Media.Application.Abstractions;

namespace Triplog.Media.Infrastructure;

public sealed class UnitOfWork(TriplogMediaDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct);
}
