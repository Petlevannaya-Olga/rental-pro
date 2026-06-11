namespace RentalPro.Contracts.Orders;

public sealed record UpdateOrderItemDto(
    Guid Id,
    DateOnly StartDate,
    DateOnly PlannedReturnDate);