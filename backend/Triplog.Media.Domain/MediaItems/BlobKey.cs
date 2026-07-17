using Triplog.Media.Domain.Common;

namespace Triplog.Media.Domain.MediaItems;

public sealed record BlobKey
{
    public string Value { get; }
    private BlobKey(string value) => Value = value;

    public static BlobKey ForOwner(OwnerId ownerId, MediaItemId mediaItemId) =>
        new($"owners/{ownerId.Value}/{mediaItemId.Value}");

    // For EF Core loading - accepts any pre-validated string
    public static BlobKey Materialize(string value) => new(value);

    public override string ToString() => Value;
}
