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
    public async Task<UnitResult<Errors>> Handle(
        ChangeToolStatusCommand command,
        CancellationToken cancellationToken)
    {
        var toolId = ToolId.Restore(command.ToolId);
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
                    $"Tool with id '{command.ToolId}' was not found")
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

        var changeStatusResult = toolResult.Value.ChangeStatus(
            command.StatusId);

        if (changeStatusResult.IsFailure)
            return changeStatusResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool status was changed. ToolId = {ToolId}, StatusId = {StatusId}",
            command.ToolId,
            command.StatusId);

        return UnitResult.Success<Errors>();
    }
}