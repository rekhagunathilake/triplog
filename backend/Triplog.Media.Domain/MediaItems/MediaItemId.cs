namespace Triplog.Media.Domain.MediaItems;

public readonly record struct MediaItemId(Guid Value)
{
    public static MediaItemId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
