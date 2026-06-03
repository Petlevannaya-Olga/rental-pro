using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Domain.Users;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.ChangeUserPasswordCommand;

public sealed class ChangeUserPasswordCommandHandler(
    IUserRepository userRepository,
    ITransactionManager transactionManager,
    ILogger<ChangeUserPasswordCommandHandler> logger)
    : ICommandHandler<ChangeUserPasswordCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        ChangeUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(command.UserId);

        if (userIdResult.IsFailure)
            return userIdResult.Error.ToErrors();

        var userResult = await userRepository.GetByAsync(
            x => x.Id == userIdResult.Value,
            cancellationToken);

        if (userResult.IsFailure)
            return userResult.Error.ToErrors();

        if (userResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "user.not.found",
                    $"User with id '{command.UserId}' was not found")
                .ToErrors();
        }

        var user = userResult.Value;

        var oldPasswordIsValid = BCrypt.Net.BCrypt.Verify(
            command.OldPassword,
            user.PasswordHash.Value);

        if (!oldPasswordIsValid)
        {
            return CommonErrors.Validation(
                    nameof(command.OldPassword),
                    "Old password is invalid")
                .ToErrors();
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);

        var changePasswordResult = user.ChangePasswordHash(newPasswordHash);

        if (changePasswordResult.IsFailure)
            return changePasswordResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Password was changed for user with id '{UserId}'",
            command.UserId);

        return UnitResult.Success<Errors>();
    }
}