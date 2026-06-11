namespace RentalPro.Presentation.Client.Models;

public sealed class OrderToolFormModel
{
    public Guid ToolId { get; set; }

    public string ToolName { get; set; } = string.Empty;

    public decimal RentalPricePerDay { get; set; }

    public decimal DepositAmount { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int RentalDays =>
        Math.Max(0, EndDate.DayNumber - StartDate.DayNumber);

    public decimal TotalAmount =>
        RentalPricePerDay * RentalDays;
}