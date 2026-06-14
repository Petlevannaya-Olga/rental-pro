namespace RentalPro.Contracts.Customers;

public sealed record ExportCustomersRequest(
    string? Search,
    bool? HasOrders,
    bool? HasActiveOrders,
    string? SortBy,
    bool Descending = false);