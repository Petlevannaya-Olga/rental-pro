using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportOrdersQuery;

public sealed record ExportOrdersQuery(
    string? Search,
    Guid? StatusId,
    DateOnly? StartFrom,
    DateOnly? StartTo,
    DateOnly? EndFrom,
    DateOnly? EndTo,
    string? SortBy,
    bool Descending)
    : IQuery;