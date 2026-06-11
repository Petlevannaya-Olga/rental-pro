namespace RentalPro.Contracts.Orders;

public sealed record ExportOrdersRequest(
    string? Search,
    Guid? StatusId,
    DateOnly? StartFrom,
    DateOnly? StartTo,
    DateOnly? EndFrom,
    DateOnly? EndTo,
    string? SortBy,
    bool Descending = false);