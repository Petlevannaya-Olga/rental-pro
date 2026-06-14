namespace RentalPro.Contracts.Customers;

public sealed record ExportCustomersRequest(
    string? Search,
    bool? HasOrders,
    bool? HasActiveOrders,
    bool? IsRegular,
    string? SortBy,
    bool Descending = false);