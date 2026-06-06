using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.ExportToolsQuery;

public sealed record ExportToolsQuery(
    string? Search,
    Guid? CategoryId,
    Guid? ManufacturerId,
    Guid? StatusId,
    string? SortBy,
    bool Descending) : IQuery;