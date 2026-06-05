using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Users;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.DeleteUserCommand;

public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    ITransactionManager transactionManager,
    ILogger<DeleteUserCommandHandler> logger)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteUserCommand command,
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

        var deleteResult = userResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "User with id '{UserId}' was deleted",
            command.UserId);

        return UnitResult.Success<Errors>();
    }
}