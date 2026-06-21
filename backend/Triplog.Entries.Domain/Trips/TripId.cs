namespace Triplog.Entries.Domain.Trips;

public readonly record struct TripId(Guid Value)
{
    public static TripId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
