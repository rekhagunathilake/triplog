using Triplog.Entries.Domain.Abstractions;

namespace Triplog.Entries.Domain.Trips;

public sealed record DateRange
{
    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }


    private DateRange(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static Result<DateRange> Create(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            return Result.Failure<DateRange>(TripErrors.DateRangeOutOfRange);

        return Result.Success(new DateRange(startDate, endDate));
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} -> {EndDate:yyyy-MM-dd}";
}
