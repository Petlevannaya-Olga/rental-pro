using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.DeleteToolCommand;

public sealed class DeleteToolCommandHandler(
    IToolsRepository toolsRepository,
    IToolStatusesRepository toolStatusesRepository,
    ITransactionManager transactionManager,
    ILogger<DeleteToolCommandHandler> logger)
    : ICommandHandler<DeleteToolCommand>
{
    private const string ReservedStatusName = "Забронирован";
    private const string RentedStatusName = "В аренде";

    public async Task<UnitResult<Errors>> Handle(
        DeleteToolCommand command,
        CancellationToken cancellationToken)
    {
        var toolId = ToolId.Restore(command.Id);

        var toolResult = await toolsRepository.GetByAsync(
            x => x.Id == toolId,
            cancellationToken);

        if (toolResult.IsFailure)
            return toolResult.Error.ToErrors();

        if (toolResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.not.found",
                    $"Инструмент с id '{command.Id}' не найден")
                .ToErrors();
        }

        var tool = toolResult.Value;

        var currentStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Id == tool.StatusId,
            cancellationToken);

        if (currentStatusResult.IsFailure)
            return currentStatusResult.Error.ToErrors();

        if (currentStatusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.current.status.not.found",
                    "Текущий статус инструмента не найден")
                .ToErrors();
        }

        var currentStatusName = currentStatusResult.Value.Name.Value;

        if (currentStatusName is ReservedStatusName or RentedStatusName)
        {
            return CommonErrors.Validation(
                    "tool.delete.forbidden.by.status",
                    "Нельзя удалить инструмент, который забронирован или находится в аренде")
                .ToErrors();
        }

        var deleteResult = tool.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool with id '{ToolId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}