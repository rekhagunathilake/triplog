using MediatR;
using Triplog.Entries.Application.Abstractions;
using Triplog.Entries.Domain.Abstractions;
using Triplog.Entries.Domain.Entries;

namespace Triplog.Entries.Application.Entries.Queries.GetEntryById;

public sealed class GetEntryByIdQueryHandler(IEntryQueries queries)
    : IRequestHandler<GetEntryByIdQuery, Result<EntryDto>>
{
    public async Task<Result<EntryDto>> Handle(GetEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await queries.GetByIdAsync(request.EntryId, request.OwnerId, cancellationToken);
        return dto is null
            ? Result.Failure<EntryDto>(EntryErrors.NotFound)
            : Result.Success(dto);
    }
}