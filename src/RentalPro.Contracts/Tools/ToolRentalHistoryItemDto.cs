namespace RentalPro.Contracts.Tools;

public sealed record ToolRentalHistoryItemDto(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerFullName,
    DateOnly StartDate,
    DateOnly PlannedReturnDate,
    DateOnly? ActualReturnedDate,
    int RentalDays,
    decimal RentalAmount,
    string OrderStatusName);