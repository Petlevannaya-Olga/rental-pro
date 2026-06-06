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
    ITransactionManager transactionManager,
    ILogger<DeleteToolCommandHandler> logger)
    : ICommandHandler<DeleteToolCommand>
{
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
                    $"Tool with id '{command.Id}' was not found")
                .ToErrors();
        }

        var deleteResult = toolResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Tool with id '{ToolId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}