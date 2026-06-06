namespace RentalPro.Presentation.Client.Models;

public sealed class FakeTool
{
    public string Name { get; init; } = string.Empty;

    public string ArticleNumber { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;

    public string InventoryNumber { get; init; } = string.Empty;

    public decimal RentalPricePerDay { get; init; }

    public decimal DepositAmount { get; init; }

    public string? CurrentCondition { get; init; }

    public string? Description { get; init; }

    public string? PhotoPath { get; init; }
}