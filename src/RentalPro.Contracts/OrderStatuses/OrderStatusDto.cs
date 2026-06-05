namespace RentalPro.Contracts.OrderStatuses;

public sealed record OrderStatusDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);