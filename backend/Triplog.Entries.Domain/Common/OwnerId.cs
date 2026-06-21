namespace Triplog.Entries.Domain.Common;

public readonly record struct OwnerId(Guid Value)
{
    public static OwnerId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
