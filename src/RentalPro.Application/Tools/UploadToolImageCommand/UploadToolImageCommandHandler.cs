using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Files;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.UploadToolImageCommand;

public sealed class UploadToolImageCommandHandler(
    IToolsRepository toolsRepository,
    ITransactionManager transactionManager,
    IFileStorage fileStorage,
    ILogger<UploadToolImageCommandHandler> logger)
    : ICommandHandler<UploadToolImageCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UploadToolImageCommand command,
        CancellationToken cancellationToken)
    {
        var toolId = ToolId.Restore(command.ToolId);

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

        var saveFileResult = await fileStorage.SaveToolImageAsync(
            command.FileStream,
            command.FileName,
            cancellationToken);

        if (saveFileResult.IsFailure)
            return saveFileResult.Error.ToErrors();

        var setPhotoResult = toolResult.Value.SetPhotoPath(
            saveFileResult.Value);

        if (setPhotoResult.IsFailure)
            return setPhotoResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Image was uploaded for tool with id '{ToolId}'",
            command.ToolId);

        return UnitResult.Success<Errors>();
    }
}