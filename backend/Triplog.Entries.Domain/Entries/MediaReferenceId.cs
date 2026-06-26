namespace Triplog.Entries.Domain.Entries;

public readonly record struct MediaReferenceId(Guid Value)
{
    public static MediaReferenceId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
