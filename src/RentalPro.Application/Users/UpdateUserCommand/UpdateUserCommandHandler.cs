using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.UpdateUserCommand;

public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITransactionManager transactionManager,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(command.Id);

        if (userIdResult.IsFailure)
            return userIdResult.Error.ToErrors();

        var roleIdResult = RoleId.Create(command.RoleId);

        if (roleIdResult.IsFailure)
            return roleIdResult.Error.ToErrors();

        var userResult = await userRepository.GetByAsync(
            x => x.Id == userIdResult.Value,
            cancellationToken);

        if (userResult.IsFailure)
            return userResult.Error.ToErrors();

        if (userResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "user.not.found",
                    $"User with id '{command.Id}' was not found")
                .ToErrors();
        }

        var roleResult = await roleRepository.GetByAsync(
            x => x.Id == roleIdResult.Value,
            cancellationToken);

        if (roleResult.IsFailure)
            return roleResult.Error.ToErrors();

        if (roleResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "role.not.found",
                    $"Role with id '{command.RoleId}' was not found")
                .ToErrors();
        }

        var updateResult = userResult.Value.Update(
            command.Login,
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.PhoneNumber,
            command.Email,
            command.RoleId);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "User with id '{UserId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}