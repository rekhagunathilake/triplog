using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries;

public sealed record MediaReference
{
    public MediaReferenceId Id { get; private set; }

    public int DisplayOrder { get; private set; }

    private MediaReference() { } // For EF Core 

    private MediaReference(MediaReferenceId id, int displayOrder)
    {
        Id = id;
        DisplayOrder = displayOrder;
    }

    public static Result<MediaReference> Create(MediaReferenceId id, int displayOrder)
    {
        if (displayOrder < 0)
            return Result.Failure<MediaReference>(EntryErrors.InvalidDisplayOrder);

        var mediaReference = new MediaReference(id, displayOrder);

        return Result.Success(mediaReference);
    }
}
