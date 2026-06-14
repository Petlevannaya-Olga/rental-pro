namespace RentalPro.Contracts.Tools;

public sealed record UpdateToolRequest(
    string ArticleNumber,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid ManufacturerId,
    decimal RentalPricePerDay,
    decimal DepositAmount,
    string SerialNumber,
    string InventoryNumber,
    string? CurrentCondition);