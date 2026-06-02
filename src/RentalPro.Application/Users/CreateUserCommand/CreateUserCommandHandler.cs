using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Extensions;
using RentalPro.Contracts.Users;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.CreateUserCommand;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ILogger<CreateUserCommandHandler> logger)
    : ICommandHandler<Guid, CreateUserCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        Users.CreateUserCommand.CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var roleIdResult = RoleId.Create(command.RoleId);

        if (roleIdResult.IsFailure)
        {
            logger.LogError(
                "User creation failed. Invalid role id: {Error}",
                roleIdResult.Error);

            return roleIdResult.Error.ToErrors();
        }

        var roleResult = await roleRepository.GetByAsync(
            x => x.Id == roleIdResult.Value,
            cancellationToken);

        if (roleResult.IsFailure)
            return roleResult.Error.ToErrors();

        if (roleResult.Value is null)
        {
            var error = CommonErrors.NotFound(
                nameof(Role),
                "Role was not found",
                command.RoleId);

            logger.LogError(
                "Cannot create user. Role with id '{RoleId}' was not found",
                command.RoleId);

            return error.ToErrors();
        }

        var loginResult = Login.Create(command.Login);

        if (loginResult.IsFailure)
        {
            logger.LogError(
                "User creation failed. Invalid login: {Error}",
                loginResult.Error);

            return loginResult.Error.ToErrors();
        }

        var existingLoginResult = await userRepository.GetByAsync(
            x => x.Login == loginResult.Value,
            cancellationToken);

        if (existingLoginResult.IsFailure)
            return existingLoginResult.Error.ToErrors();

        if (existingLoginResult.Value is not null)
        {
            var error = CommonErrors.Conflict(
                nameof(User.Login),
                $"User with login '{command.Login}' already exists");

            logger.LogError(
                "Cannot create user with login '{Login}' because it already exists",
                command.Login);

            return error.ToErrors();
        }

        var emailResult = Email.Create(command.Email);

        if (emailResult.IsFailure)
        {
            logger.LogError(
                "User creation failed. Invalid email: {Error}",
                emailResult.Error);

            return emailResult.Error.ToErrors();
        }

        var existingEmailResult = await userRepository.GetByAsync(
            x => x.Email == emailResult.Value,
            cancellationToken);

        if (existingEmailResult.IsFailure)
            return existingEmailResult.Error.ToErrors();

        if (existingEmailResult.Value is not null)
        {
            var error = CommonErrors.Conflict(
                nameof(User.Email),
                $"User with email '{command.Email}' already exists");

            logger.LogError(
                "Cannot create user with email '{Email}' because it already exists",
                command.Email);

            return error.ToErrors();
        }

        var phoneNumberResult = PhoneNumber.Create(command.PhoneNumber);

        if (phoneNumberResult.IsFailure)
        {
            logger.LogError(
                "User creation failed. Invalid phone number: {Error}",
                phoneNumberResult.Error);

            return phoneNumberResult.Error.ToErrors();
        }

        var existingPhoneNumberResult = await userRepository.GetByAsync(
            x => x.PhoneNumber == phoneNumberResult.Value,
            cancellationToken);

        if (existingPhoneNumberResult.IsFailure)
            return existingPhoneNumberResult.Error.ToErrors();

        if (existingPhoneNumberResult.Value is not null)
        {
            var error = CommonErrors.Conflict(
                nameof(User.PhoneNumber),
                $"User with phone number '{command.PhoneNumber}' already exists");

            logger.LogError(
                "Cannot create user with phone number '{PhoneNumber}' because it already exists",
                command.PhoneNumber);

            return error.ToErrors();
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

        var userResult = User.Create(
            command.Login,
            passwordHash,
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.PhoneNumber,
            command.Email,
            command.RoleId);

        if (userResult.IsFailure)
        {
            logger.LogError(
                "User creation failed: {Error}",
                userResult.Error);

            return userResult.Error.ToErrors();
        }

        var addResult = await userRepository.AddAsync(
            userResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "User created successfully. UserId = {UserId}",
            userResult.Value.Id.Value);

        return userResult.Value.Id.Value;
    }
}