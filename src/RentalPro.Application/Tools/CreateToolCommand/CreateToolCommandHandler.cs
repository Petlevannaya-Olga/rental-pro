using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.CreateToolCommand;

public sealed class CreateToolCommandHandler(
    IToolsRepository toolsRepository,
    IToolCategoriesRepository toolCategoriesRepository,
    IManufacturerRepository manufacturersRepository,
    IToolStatusesRepository toolStatusesRepository,
    ILogger<CreateToolCommandHandler> logger)
    : ICommandHandler<Guid, CreateToolCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreateToolCommand command,
        CancellationToken cancellationToken)
    {
        var categoryId = ToolCategoryId.Restore(command.CategoryId);
        var manufacturerId = ManufacturerId.Restore(command.ManufacturerId);
        var statusId = ToolStatusId.Restore(command.StatusId);

        var categoryResult = await toolCategoriesRepository.GetByAsync(
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

        var manufacturerResult = await manufacturersRepository.GetByAsync(
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

        var statusResult = await toolStatusesRepository.GetByAsync(
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

        var toolResult = Tool.Create(
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
            photoPath: null);

        if (toolResult.IsFailure)
            return toolResult.Error.ToErrors();

        var addResult = await toolsRepository.AddAsync(
            toolResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Tool '{ToolName}' was created. Id = {ToolId}",
            addResult.Value.Name.Value,
            addResult.Value.Id.Value);

        return addResult.Value.Id.Value;
    }
}