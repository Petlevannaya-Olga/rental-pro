using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Roles;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.UpdateRoleCommand;

public sealed class UpdateRoleCommandHandler(
    IRoleRepository repository,
    ITransactionManager transactionManager,
    ILogger<UpdateRoleCommandHandler> logger)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var roleId = RoleId.Restore(command.Id);

        var roleResult = await repository.GetByAsync(
            x => x.Id == roleId,
            cancellationToken);

        if (roleResult.IsFailure)
            return roleResult.Error.ToErrors();

        if (roleResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "role.not.found",
                    $"Role with id '{command.Id}' was not found")
                .ToErrors();
        }

        var updateResult = roleResult.Value.Update(command.Name);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Role with id '{RoleId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}