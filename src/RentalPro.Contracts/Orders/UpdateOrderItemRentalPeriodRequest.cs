namespace RentalPro.Contracts.Orders;

public sealed record UpdateOrderItemRentalPeriodRequest(
    DateOnly StartDate,
    DateOnly PlannedReturnDate);