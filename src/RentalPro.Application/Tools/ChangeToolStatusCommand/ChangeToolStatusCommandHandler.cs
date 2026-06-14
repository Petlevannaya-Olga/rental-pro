using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.ChangeToolStatusCommand;

public sealed class ChangeToolStatusCommandHandler(
    IToolsRepository toolsRepository,
    IToolStatusesRepository toolStatusesRepository,
    ITransactionManager transactionManager,
    ILogger<ChangeToolStatusCommandHandler> logger)
    : ICommandHandler<ChangeToolStatusCommand>
{
    private const string AvailableStatusName = "Доступен";
    private const string RepairStatusName = "На ремонте";
    private const string WrittenOffStatusName = "Списан";

    public async Task<UnitResult<Errors>> Handle(
        ChangeToolStatusCommand command,
        CancellationToken cancellationToken)
    {
        var toolId = ToolId.Restore(command.ToolId);
        var newStatusId = ToolStatusId.Restore(command.StatusId);

        var toolResult = await toolsRepository.GetByAsync(
            x => x.Id == toolId,
            cancellationToken);

        if (toolResult.IsFailure)
            return toolResult.Error.ToErrors();

        if (toolResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.not.found",
                    $"Инструмент с id '{command.ToolId}' не найден")
                .ToErrors();
        }

        var tool = toolResult.Value;

        var newStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Id == newStatusId,
            cancellationToken);

        if (newStatusResult.IsFailure)
            return newStatusResult.Error.ToErrors();

        if (newStatusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.status.not.found",
                    $"Статус инструмента с id '{command.StatusId}' не найден")
                .ToErrors();
        }

        var newStatus = newStatusResult.Value;

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

        var currentStatus = currentStatusResult.Value;

        var validationResult = ValidateManualStatusChange(
            currentStatus.Name.Value,
            newStatus.Name.Value);

        if (validationResult.IsFailure)
            return validationResult.Error.ToErrors();

        var changeStatusResult = tool.ChangeStatus(
            command.StatusId);

        if (changeStatusResult.IsFailure)
            return changeStatusResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool status was changed. ToolId = {ToolId}, OldStatusId = {OldStatusId}, NewStatusId = {NewStatusId}",
            command.ToolId,
            currentStatus.Id.Value,
            command.StatusId);

        return UnitResult.Success<Errors>();
    }

    private static UnitResult<Error> ValidateManualStatusChange(
        string currentStatusName,
        string newStatusName)
    {
        if (currentStatusName == AvailableStatusName &&
            newStatusName == RepairStatusName)
        {
            return UnitResult.Success<Error>();
        }

        if (currentStatusName == RepairStatusName &&
            newStatusName == AvailableStatusName)
        {
            return UnitResult.Success<Error>();
        }

        if (currentStatusName == RepairStatusName &&
            newStatusName == WrittenOffStatusName)
        {
            return UnitResult.Success<Error>();
        }

        return CommonErrors.Validation(
            "tool.manual.status.change.forbidden",
            "Через страницу инструментов доступны только переходы: доступен → на ремонте, на ремонте → доступен, на ремонте → списан");
    }
}