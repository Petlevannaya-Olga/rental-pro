using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.CreateToolStatusCommand;

public sealed class CreateToolStatusCommandHandler(
    IToolStatusesRepository repository,
    ILogger<CreateToolStatusCommandHandler> logger)
    : ICommandHandler<CreateToolStatusCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreateToolStatusCommand command,
        CancellationToken cancellationToken)
    {
        var toolStatusResult = ToolStatus.Create(command.Name);

        if (toolStatusResult.IsFailure)
            return toolStatusResult.Error.ToErrors();

        var addResult = await repository.AddAsync(
            toolStatusResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Tool status '{ToolStatusName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}