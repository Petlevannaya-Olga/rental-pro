namespace RentalPro.Presentation.Desktop.Models;

public sealed class SelectedToolModel
{
    public Guid ToolId { get; init; }

    public string ToolName { get; init; } = string.Empty;

    public decimal RentalPricePerDay { get; init; }

    public decimal DepositAmount { get; init; }

    public string InventoryNumber { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;
}