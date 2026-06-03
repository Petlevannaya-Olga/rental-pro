using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.ChangeUserStatusCommand;

public sealed class ChangeUserStatusCommandHandler(
    IUserRepository userRepository,
    ITransactionManager transactionManager,
    ILogger<ChangeUserStatusCommandHandler> logger)
    : ICommandHandler<ChangeUserStatusCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        var userResult = await userRepository.GetByAsync(
            x => x.Id == command.UserId,
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

        var statusResult = command.IsActive
            ? user.Activate()
            : user.Deactivate();

        if (statusResult.IsFailure)
        {
            logger.LogError(
                "Failed to change user status. UserId: {UserId}. Error: {Error}",
                command.UserId,
                statusResult.Error);

            return statusResult.Error.ToErrors();
        }

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "User status changed. UserId: {UserId}. IsActive: {IsActive}",
            command.UserId,
            command.IsActive);

        return UnitResult.Success<Errors>();
    }
}