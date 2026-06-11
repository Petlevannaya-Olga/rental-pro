namespace RentalPro.Contracts.Orders;

public sealed record GetOrdersRequest(
    string? Search,
    Guid? StatusId,
    DateOnly? StartFrom,
    DateOnly? StartTo,
    DateOnly? EndFrom,
    DateOnly? EndTo,
    string? SortBy,
    bool Descending = false,
    int Page = 1,
    int PageSize = 10);