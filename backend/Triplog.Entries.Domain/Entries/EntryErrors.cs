using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries;

public static class EntryErrors
{
    public static readonly Error TitleEmpty =
        Error.Validation("Entry.TitleEmpty", "Entry title cannot be empty.");

    public static Error TitleTooLong(int actualLength) => 
        Error.Validation("Entry.TitleTooLong", $"Entry title length {actualLength} exceeds the maximum of {EntryTitle.MaxLength}.");

    public static readonly Error BodyEmpty = 
        Error.Validation("Entry.BodyEmpty", "Entry body cannot be empty.");

    public static Error BodyTooLong(int actualLength) => 
        Error.Validation("Entry.BodyTooLong", $"Entry body length {actualLength} exceeds the maximum of {EntryBody.MaxLength}.");

    public static readonly Error InvalidDisplayOrder = 
        Error.Validation("Entry.InvalidDisplayOrder", "Entry display order must be zero or greater.");

    public static readonly Error IsArchived =
            Error.Validation("Entry.IsArchived", "Cannot modify an archived entry.");

    public static readonly Error NotDraft =
            Error.Validation("Entry.NotDraft", "Only an entry in Draft status can be modified.");

    public static readonly Error NoMediaAttached =
            Error.Validation("Entry.NoMediaAttached", "Entry must have at least one media item in order to be published.");

    public static readonly Error MediaNotFound =
            Error.Validation("Entry.MediaNotFound", "Media item not found in this entry.");

    public static readonly Error MediaAlreadyAttached =
            Error.Validation("Entry.MediaAlreadyAttached", "Media item is already attached to this entry.");

    public static readonly Error InvalidStatusTransition = 
        Error.Validation("Entry.InvalidStatusTransition", "The requested status transition is not allowed from the current status.");

    public static readonly Error AlreadyArchived = 
        Error.Validation("Entry.AlreadyArchived", "Entry is already archived.");
}
