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
    private const string AvailableStatusName = "Доступен";

    public async Task<Result<Guid, Errors>> Handle(
        CreateToolCommand command,
        CancellationToken cancellationToken)
    {
        var categoryId = ToolCategoryId.Restore(command.CategoryId);
        var manufacturerId = ManufacturerId.Restore(command.ManufacturerId);

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

        var availableStatusNameResult = ToolStatusName.Create(AvailableStatusName);

        if (availableStatusNameResult.IsFailure)
            return availableStatusNameResult.Error.ToErrors();

        var availableStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Name == availableStatusNameResult.Value,
            cancellationToken);

        if (availableStatusResult.IsFailure)
            return availableStatusResult.Error.ToErrors();

        if (availableStatusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.status.available.not.found",
                    $"Tool status '{AvailableStatusName}' was not found")
                .ToErrors();
        }

        var availableStatusId = availableStatusResult.Value.Id.Value;

        var toolResult = Tool.Create(
            articleNumber: command.ArticleNumber,
            name: command.Name,
            description: command.Description,
            categoryId: command.CategoryId,
            manufacturerId: command.ManufacturerId,
            statusId: availableStatusId,
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
            "Tool '{ToolName}' was created with status '{StatusName}'. Id = {ToolId}",
            addResult.Value.Name.Value,
            AvailableStatusName,
            addResult.Value.Id.Value);

        return addResult.Value.Id.Value;
    }
}