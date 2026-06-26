using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Entries;

public sealed record EntryTitle
{
    public const int MaxLength = 200;

    public string Value { get; }

    private EntryTitle(string value) => Value = value;

    public static Result<EntryTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<EntryTitle>(EntryErrors.TitleEmpty);

        if (value.Length > MaxLength)
            return Result.Failure<EntryTitle>(EntryErrors.TitleTooLong(value.Length));

        return Result.Success(new EntryTitle(value));
    }

    public override string ToString() => Value;
}
