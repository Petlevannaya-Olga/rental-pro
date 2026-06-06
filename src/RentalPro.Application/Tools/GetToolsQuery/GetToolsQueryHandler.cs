using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Tools;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.GetToolsQuery;

public sealed class GetToolsQueryHandler(
    IToolsReadRepository readRepository)
    : IQueryHandler<PagedResult<ToolDto>, GetToolsQuery>
{
    public async Task<Result<PagedResult<ToolDto>, Errors>> Handle(
        GetToolsQuery query,
        CancellationToken cancellationToken)
    {
        var categoryIdResult = CreateToolCategoryId(query.CategoryId);

        if (categoryIdResult.IsFailure)
            return categoryIdResult.Error.ToErrors();

        var manufacturerIdResult = CreateManufacturerId(query.ManufacturerId);

        if (manufacturerIdResult.IsFailure)
            return manufacturerIdResult.Error.ToErrors();

        var statusIdResult = CreateToolStatusId(query.StatusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error.ToErrors();

        return await readRepository.GetPagedAsync(
            query.Search,
            categoryIdResult.Value,
            manufacturerIdResult.Value,
            statusIdResult.Value,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    private static Result<ToolCategoryId?, Error> CreateToolCategoryId(
        Guid? categoryId)
    {
        if (categoryId is null)
            return (ToolCategoryId?)null;

        var result = ToolCategoryId.Create(categoryId.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }

    private static Result<ManufacturerId?, Error> CreateManufacturerId(
        Guid? manufacturerId)
    {
        if (manufacturerId is null)
            return (ManufacturerId?)null;

        var result = ManufacturerId.Create(manufacturerId.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }

    private static Result<ToolStatusId?, Error> CreateToolStatusId(
        Guid? statusId)
    {
        if (statusId is null)
            return (ToolStatusId?)null;

        var result = ToolStatusId.Create(statusId.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}