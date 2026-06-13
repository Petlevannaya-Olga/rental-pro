namespace RentalPro.Contracts.Orders;

public sealed record ReturnOrderItemsRequest(
    DateOnly ActualReturnedDate,
    string? ReturnCondition,
    string? DamageComment,
    IReadOnlyList<Guid> OrderItemIds);