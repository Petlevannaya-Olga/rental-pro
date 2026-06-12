namespace RentalPro.Contracts.Orders;

public sealed record TransferActItemDto(
    Guid OrderItemId,
    Guid ToolId,
    string ToolName,
    string InventoryNumber,
    string SerialNumber,
    DateOnly StartDate,
    DateOnly PlannedReturnDate,
    int RentalDays,
    decimal RentalPrice,
    decimal DepositAmount,
    string Condition,
    string? Comment);