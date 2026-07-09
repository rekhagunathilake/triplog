using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Common;

public sealed record Location
{
    public const int MaxLocationNameLength = 200;

    public string Name { get; }

    public double Latitude { get; }

    public double Longitude { get; }

    public Location(string name, double latitude, double longitude)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<Location> Create(string name, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Location>(LocationErrors.NameEmpty);

        if (name.Length > MaxLocationNameLength)
            return Result.Failure<Location>(LocationErrors.NameTooLong(name.Length));

        if (latitude is < -90 or > 90)
            return Result.Failure<Location>(LocationErrors.LatitudeOutOfRange(latitude));

        if (longitude is < -180 or > 180)
            return Result.Failure<Location>(LocationErrors.LongitudeOutOfRange(longitude));

        return Result.Success(new Location(name.Trim(), latitude, longitude));
    }

    public override string ToString() => $"{Name} ({Latitude:F4}, {Longitude:F4})";
}
