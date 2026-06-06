namespace RentalPro.Contracts.Tools;

public sealed record GetToolsRequest(
    string? Search,
    Guid? CategoryId,
    Guid? ManufacturerId,
    Guid? StatusId,
    string? SortBy,
    bool Descending,
    int Page = 1,
    int PageSize = 10);