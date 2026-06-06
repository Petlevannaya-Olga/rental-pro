namespace RentalPro.Contracts.Tools;

public sealed record ToolDto(
    Guid Id,
    string ArticleNumber,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    Guid ManufacturerId,
    string ManufacturerName,
    Guid StatusId,
    string StatusName,
    decimal RentalPricePerDay,
    decimal DepositAmount,
    string SerialNumber,
    string InventoryNumber,
    string? CurrentCondition,
    string? PhotoPath,
    DateTime CreatedAt,
    DateTime? UpdatedAt);