namespace RentalPro.Contracts.Orders;

public sealed record OrderDetailsDto(
    Guid Id,
    string Number,

    Guid CustomerId,
    string CustomerFullName,

    Guid UserId,
    string UserFullName,

    Guid StatusId,
    string StatusName,

    DateTime OrderDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,

    string? Comment,
    
    decimal TotalCost,
    decimal DepositTotal,

    List<OrderDetailsItemDto> Items);