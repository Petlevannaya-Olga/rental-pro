using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class RentalPeriod : ValueObject
{
    public DateOnly StartDate { get; }
    public DateOnly PlannedReturnDate { get; }

    private RentalPeriod(DateOnly startDate, DateOnly plannedReturnDate)
    {
        StartDate = startDate;
        PlannedReturnDate = plannedReturnDate;
    }

    public int DaysCount => PlannedReturnDate.DayNumber - StartDate.DayNumber + 1;

    public static Result<RentalPeriod, Error> Create(
        DateOnly startDate,
        DateOnly plannedReturnDate)
    {
        if (plannedReturnDate < startDate)
        {
            return CommonErrors.Validation(
                nameof(plannedReturnDate),
                "Planned return date cannot be earlier than start date");
        }

        return new RentalPeriod(startDate, plannedReturnDate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return PlannedReturnDate;
    }
}