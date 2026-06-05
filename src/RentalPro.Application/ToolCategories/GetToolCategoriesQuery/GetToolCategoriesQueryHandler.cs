using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.ToolCategories;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.GetToolCategoriesQuery;

public sealed class GetToolCategoriesQueryHandler(
    IToolCategoriesRepository repository)
    : IQueryHandler<IReadOnlyList<ToolCategoryDto>, GetToolCategoriesQuery>
{
    public async Task<Result<IReadOnlyList<ToolCategoryDto>, Errors>> Handle(
        GetToolCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => new ToolCategoryDto(
                x.Id.Value,
                x.Name.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();
    }
}