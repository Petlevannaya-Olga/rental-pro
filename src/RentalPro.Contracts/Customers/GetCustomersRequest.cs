namespace RentalPro.Contracts.Customers;

public sealed record GetCustomersRequest(
    string? Search,
    bool? HasOrders,
    bool? HasDebt,
    string? SortBy,
    bool Descending,
    int Page = 1,
    int PageSize = 10);