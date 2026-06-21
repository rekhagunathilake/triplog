using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Common;

public static class LocationErrors
{
    public static readonly Error NameEmpty = Error.Validation(
        "Location.NameEmpty","Location name cannot be empty.");

    public static Error NameTooLong(int actualLength) =>
        Error.Validation("Location.NameTooLong", 
            $"Location name length {actualLength} exceeds the maximum of {Location.MaxLocationNameLength}.");

    public static Error LatitudeOutOfRange(double value) =>
        Error.Validation("Location.LatitudeOutOfRange",
            $"Latitude {value} must be between -90 and 90 inclusive.");

    public static Error LongitudeOutOfRange(double value) =>
        Error.Validation("Location.LongitudeOutOfRange",
            $"Longitude {value} must be between -180 and 180 inclusive.");
}
