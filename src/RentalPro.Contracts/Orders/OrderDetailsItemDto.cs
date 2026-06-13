namespace RentalPro.Contracts.Orders;

public sealed record OrderDetailsItemDto(
    Guid Id,
    Guid ToolId,
    string ToolName,

    DateOnly StartDate,
    DateOnly PlannedReturnDate,
    DateOnly? ActualReturnedDate,

    decimal RentalPricePerDay,
    decimal TotalAmount,
    decimal DepositAmount);