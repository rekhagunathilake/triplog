using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Abstractions;

public interface IEntryRepository
{
    Task AddAsync(Entry entry, CancellationToken ct = default);

    Task<Entry?> GetByIdAsync(EntryId id, CancellationToken ct = default);
}
