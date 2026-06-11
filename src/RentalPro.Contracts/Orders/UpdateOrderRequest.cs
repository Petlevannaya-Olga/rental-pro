namespace RentalPro.Contracts.Orders;

public sealed record UpdateOrderRequest(
    Guid StatusId,
    string? Comment);