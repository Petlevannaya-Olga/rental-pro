using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.DeleteToolCategoriesCommand;

public sealed class DeleteToolCategoryCommandHandler(
    IToolCategoriesRepository repository,
    ITransactionManager transactionManager,
    ILogger<DeleteToolCategoryCommandHandler> logger)
    : ICommandHandler<DeleteToolCategoryCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteToolCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var categoryId = ToolCategoryId.Restore(command.Id);

        var categoryResult = await repository.GetByAsync(
            x => x.Id == categoryId,
            cancellationToken);

        if (categoryResult.IsFailure)
            return categoryResult.Error.ToErrors();

        if (categoryResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.category.not.found",
                    $"Tool category with id '{command.Id}' was not found")
                .ToErrors();
        }

        var deleteResult = categoryResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool category with id '{ToolCategoryId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}