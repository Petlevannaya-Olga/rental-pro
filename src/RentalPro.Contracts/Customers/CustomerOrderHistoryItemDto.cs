namespace RentalPro.Contracts.Customers;

public sealed record CustomerOrderHistoryItemDto(
    Guid OrderId,
    string OrderNumber,
    DateTime OrderDate,
    int ToolsCount,
    DateOnly? StartDate,
    string StatusName,
    decimal RentalAmount,
    decimal DepositAmount);