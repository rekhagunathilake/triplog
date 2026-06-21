using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Trips;

public sealed record TripTitle
{
    public const int MaxLength = 200;

    public string Value { get; }

    private TripTitle(string value) => Value = value;

    public static Result<TripTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<TripTitle>(TripErrors.TitleEmpty);

        if (value.Length > MaxLength)
            return Result.Failure<TripTitle>(TripErrors.TitleTooLong(value.Length));

        return Result.Success(new TripTitle(value));
    }

    public override string ToString() => Value;
}
