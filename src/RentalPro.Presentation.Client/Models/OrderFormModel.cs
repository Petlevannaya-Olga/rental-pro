namespace RentalPro.Presentation.Client.Models;

public sealed class OrderFormModel
{
    public Guid? CustomerId { get; set; }

    public List<OrderToolFormModel> Tools { get; set; } = [];

    public string CustomerName { get; set; } = string.Empty;

    public string ToolName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public string? Comment { get; set; }
}