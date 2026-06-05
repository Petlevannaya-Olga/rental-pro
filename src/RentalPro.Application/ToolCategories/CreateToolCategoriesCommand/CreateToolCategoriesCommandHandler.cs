using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.CreateToolCategoriesCommand;

public sealed class CreateToolCategoryCommandHandler(
    IToolCategoriesRepository repository,
    ILogger<CreateToolCategoryCommandHandler> logger)
    : ICommandHandler<CreateToolCategoryCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreateToolCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var categoryResult = ToolCategory.Create(command.Name);

        if (categoryResult.IsFailure)
            return categoryResult.Error.ToErrors();

        var addResult = await repository.AddAsync(
            categoryResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Tool category '{ToolCategoryName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}