namespace RentalPro.Presentation.Desktop.Models;

public sealed class ToolEditModel
{
    public string ArticleNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? ManufacturerId { get; set; }

    public Guid? StatusId { get; set; }

    public decimal RentalPricePerDay { get; set; }

    public decimal DepositAmount { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public string InventoryNumber { get; set; } = string.Empty;

    public string? CurrentCondition { get; set; }

    public string? PhotoPath { get; set; }
}