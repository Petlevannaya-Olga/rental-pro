namespace RentalPro.Contracts.Orders;

public sealed record OrderDetailsItemDto(
    Guid ToolId,
    string ToolName,

    DateOnly StartDate,
    DateOnly PlannedReturnDate,

    decimal RentalPricePerDay,
    decimal TotalAmount,
    decimal DepositAmount);