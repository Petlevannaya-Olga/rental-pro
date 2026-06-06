using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.UpdateToolCommand;

public sealed class UpdateToolCommandHandler(
    IToolsRepository toolsRepository,
    IToolCategoriesRepository toolCategoryRepository,
    IManufacturerRepository manufacturerRepository,
    IToolStatusesRepository toolStatusRepository,
    ITransactionManager transactionManager,
    ILogger<UpdateToolCommandHandler> logger)
    : ICommandHandler<UpdateToolCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateToolCommand command,
        CancellationToken cancellationToken)
    {
        var toolId = ToolId.Restore(command.Id);
        var categoryId = ToolCategoryId.Restore(command.CategoryId);
        var manufacturerId = ManufacturerId.Restore(command.ManufacturerId);
        var statusId = ToolStatusId.Restore(command.StatusId);

        var toolResult = await toolsRepository.GetByAsync(
            x => x.Id == toolId,
            cancellationToken);

        if (toolResult.IsFailure)
            return toolResult.Error.ToErrors();

        if (toolResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.not.found",
                    $"Tool with id '{command.Id}' was not found")
                .ToErrors();
        }

        var categoryResult = await toolCategoryRepository.GetByAsync(
            x => x.Id == categoryId,
            cancellationToken);

        if (categoryResult.IsFailure)
            return categoryResult.Error.ToErrors();

        if (categoryResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.category.not.found",
                    $"Tool category with id '{command.CategoryId}' was not found")
                .ToErrors();
        }

        var manufacturerResult = await manufacturerRepository.GetByAsync(
            x => x.Id == manufacturerId,
            cancellationToken);

        if (manufacturerResult.IsFailure)
            return manufacturerResult.Error.ToErrors();

        if (manufacturerResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "manufacturer.not.found",
                    $"Manufacturer with id '{command.ManufacturerId}' was not found")
                .ToErrors();
        }

        var statusResult = await toolStatusRepository.GetByAsync(
            x => x.Id == statusId,
            cancellationToken);

        if (statusResult.IsFailure)
            return statusResult.Error.ToErrors();

        if (statusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.status.not.found",
                    $"Tool status with id '{command.StatusId}' was not found")
                .ToErrors();
        }

        var updateResult = toolResult.Value.UpdateMainInfo(
            articleNumber: command.ArticleNumber,
            name: command.Name,
            description: command.Description,
            categoryId: command.CategoryId,
            manufacturerId: command.ManufacturerId,
            statusId: command.StatusId,
            rentalPricePerDay: command.RentalPricePerDay,
            depositAmount: command.DepositAmount,
            serialNumber: command.SerialNumber,
            inventoryNumber: command.InventoryNumber,
            currentCondition: command.CurrentCondition,
            photoPath: toolResult.Value.PhotoPath?.Value);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool with id '{ToolId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}