using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Roles;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.CreateRoleCommand;

public sealed class CreateRoleCommandHandler(
    IRoleRepository repository,
    ILogger<CreateRoleCommandHandler> logger)
    : ICommandHandler<CreateRoleCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var roleResult = Role.Create(command.Name);

        if (roleResult.IsFailure)
            return roleResult.Error.ToErrors();

        var addResult = await repository.AddAsync(
            roleResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Role '{RoleName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}