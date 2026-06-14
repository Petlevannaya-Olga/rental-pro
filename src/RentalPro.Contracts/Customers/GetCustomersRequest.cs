namespace RentalPro.Contracts.Customers;

public sealed record GetCustomersRequest(
    string? Search,
    bool? HasOrders,
    bool? IsRegular,
    bool? HasActiveOrders,
    string? SortBy,
    bool Descending = false,
    int Page = 1,
    int PageSize = 10);