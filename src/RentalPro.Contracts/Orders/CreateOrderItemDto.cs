namespace RentalPro.Contracts.Orders;

public sealed record CreateOrderItemDto(
    Guid ToolId,
    decimal RentalPricePerDay,
    decimal DepositAmount,
    DateOnly StartDate,
    DateOnly PlannedReturnDate);