using Microsoft.EntityFrameworkCore;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Infrastructure.Persistence.Repositories
{
    public sealed class EntryRepository(TriplogEntriesDbContext dbContext) : IEntryRepository
    {
        public async Task AddAsync(Entry entry, CancellationToken ct = default) => await dbContext.Entries.AddAsync(entry, ct).AsTask();

        public async Task<Entry?> GetByIdAsync(EntryId id, CancellationToken ct = default) =>
            await dbContext.Entries
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
