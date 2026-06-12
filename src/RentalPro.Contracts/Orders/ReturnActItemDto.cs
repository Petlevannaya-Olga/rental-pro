namespace RentalPro.Contracts.Orders;

public sealed record ReturnActItemDto(
    Guid OrderItemId,
    Guid ToolId,
    string ToolName,
    string InventoryNumber,
    string SerialNumber,
    DateOnly StartDate,
    DateOnly PlannedReturnDate,
    DateOnly ActualReturnedDate,
    string ReturnCondition,
    string DamageComment);