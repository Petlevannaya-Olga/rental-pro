namespace RentalPro.Contracts.Orders;

public sealed record OrderDto(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerFullName,
    Guid UserId,
    string UserFullName,
    Guid StatusId,
    string StatusName,
    int ItemsCount,
    string ToolsNames,
    DateTime OrderDate,
    DateOnly? StartDate,
    DateOnly? PlannedReturnDate,
    decimal TotalCost,
    decimal DepositTotal,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt);