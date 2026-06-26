using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries;

public sealed record EntryBody
{
    public const int MaxLength = 50000;

    public string Value { get; }

    private EntryBody(string value) => Value = value;

    public static Result<EntryBody> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<EntryBody>(EntryErrors.BodyEmpty);

        if (value.Length > MaxLength)
            return Result.Failure<EntryBody>(EntryErrors.BodyTooLong(value.Length));

        return Result.Success(new EntryBody(value));
    }

    public override string ToString() => Value;
}
