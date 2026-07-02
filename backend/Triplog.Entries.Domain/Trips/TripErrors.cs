using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Trips;

public static class TripErrors
{
    public static readonly Error TitleEmpty = 
        Error.Validation("Trip.TitleEmpty", "Trip title cannot be empty.");

    public static Error TitleTooLong(int actualLength) => 
        Error.Validation("Trip.TitleTooLong", $"Trip title length {actualLength} exceeds the maximum of {TripTitle.MaxLength}.");

    public static readonly Error DateRangeOutOfRange =
        Error.Validation("Trip.DateRangeOutOfRange", "Trip start date must be on or before the end date.");

    public static readonly Error IsArchived =
            Error.Validation("Trip.IsArchived", "Cannot modify an archived trip.");

    public static readonly Error InvalidStatusTransition = 
        Error.Validation("Trip.InvalidStatusTransition", "The requested status transition is not allowed from the current status.");

    public static readonly Error AlreadyArchived = 
        Error.Validation("Trip.AlreadyArchived", "Trip is already archived.");

    public static readonly Error NotFound = 
        Error.Validation("Trip.NotFound", "Trip not found.");
}
