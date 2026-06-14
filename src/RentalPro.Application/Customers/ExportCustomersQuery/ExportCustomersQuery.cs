using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.ExportCustomersQuery;

public sealed record ExportCustomersQuery(
    string? Search,
    bool? HasOrders,
    bool? IsRegular,
    bool? HasActiveOrders,
    string? SortBy,
    bool Descending)
    : IQuery;