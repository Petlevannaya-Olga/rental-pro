using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrdersQuery;

public sealed record GetOrdersQuery(
    string? Search,
    Guid? StatusId,
    DateOnly? StartFrom,
    DateOnly? StartTo,
    DateOnly? EndFrom,
    DateOnly? EndTo,
    string? SortBy,
    bool Descending,
    int Page,
    int PageSize)
    : IQuery;