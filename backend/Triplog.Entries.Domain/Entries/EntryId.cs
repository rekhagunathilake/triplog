namespace Triplog.Entries.Domain.Entries;

public readonly record struct EntryId(Guid Value)
{
    public static EntryId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
