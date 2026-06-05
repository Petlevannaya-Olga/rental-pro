using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.UpdateToolStatusCommand;

public sealed class UpdateToolStatusCommandHandler(
    IToolStatusesRepository repository,
    ITransactionManager transactionManager,
    ILogger<UpdateToolStatusCommandHandler> logger)
    : ICommandHandler<UpdateToolStatusCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateToolStatusCommand command,
        CancellationToken cancellationToken)
    {
        var toolStatusId = ToolStatusId.Restore(command.Id);

        var toolStatusResult = await repository.GetByAsync(
            x => x.Id == toolStatusId,
            cancellationToken);

        if (toolStatusResult.IsFailure)
            return toolStatusResult.Error.ToErrors();

        if (toolStatusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "tool.status.not.found",
                    $"Tool status with id '{command.Id}' was not found")
                .ToErrors();
        }

        var updateResult = toolStatusResult.Value.Update(command.Name);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool status with id '{ToolStatusId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}