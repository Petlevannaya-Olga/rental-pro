namespace RentalPro.Presentation.Desktop.ViewModels;

public sealed class OrderItemViewModel
{
    public required string ToolName { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly PlannedReturnDate { get; init; }

    public DateOnly? ActualReturnedDate { get; init; }

    public decimal RentalPricePerDay { get; init; }

    public decimal TotalAmount { get; init; }

    public decimal DepositAmount { get; init; }

    public int RentalDays
    {
        get
        {
            var end = ActualReturnedDate ?? PlannedReturnDate;
            return Math.Max(1, end.DayNumber - StartDate.DayNumber);
        }
    }
}