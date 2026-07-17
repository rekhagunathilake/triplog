using Triplog.Media.Domain.Abstractions;

namespace Triplog.Media.Domain.MediaItems;

public sealed record ContentType
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

    public string Value { get; }
    private ContentType(string value) => Value = value;

    public static Result<ContentType> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<ContentType>(MediaItemErrors.ContentTypeEmpty);
        if (!Allowed.Contains(value))
            return Result.Failure<ContentType>(MediaItemErrors.ContentTypeNotAllowed(value));
        return Result.Success(new ContentType(value.ToLowerInvariant()));
    }

    public override string ToString() => Value;
}
