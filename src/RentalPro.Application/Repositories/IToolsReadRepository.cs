using CSharpFunctionalExtensions;
using RentalPro.Contracts.Tools;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IToolsReadRepository
{
    Task<Result<PagedResult<ToolDto>, Errors>> GetPagedAsync(
        string? search,
        ToolCategoryId? categoryId,
        ManufacturerId? manufacturerId,
        ToolStatusId? statusId,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<ToolStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ToolDto>, Errors>> GetForExportAsync(
        string? search,
        ToolCategoryId? categoryId,
        ManufacturerId? manufacturerId,
        ToolStatusId? statusId,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);
    
    Task<Result<List<ToolRentalHistoryItemDto>, Errors>> GetRentalHistoryAsync(
        Guid toolId,
        CancellationToken cancellationToken = default);
}