using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomersQuery;

public sealed record GetCustomersQuery(
    string? Search,
    bool? HasOrders,
    bool? IsRegular,
    bool? HasActiveOrders,
    string? SortBy,
    bool Descending,
    int Page,
    int PageSize)
    : IQuery;