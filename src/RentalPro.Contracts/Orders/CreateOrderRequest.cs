namespace RentalPro.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid UserId,
    Guid CustomerId,
    Guid StatusId,
    DateTime OrderDate,
    decimal DepositTotal,
    string? Comment,
    IReadOnlyList<CreateOrderItemDto> Items);