using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.GetToolsQuery;

public sealed record GetToolsQuery(
    string? Search,
    Guid? CategoryId,
    Guid? ManufacturerId,
    Guid? StatusId,
    string? SortBy,
    bool Descending,
    int Page = 1,
    int PageSize = 10) : IQuery;