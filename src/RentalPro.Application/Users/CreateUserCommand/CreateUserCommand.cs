using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.CreateUserCommand;

public sealed record CreateUserCommand(
    string Login,
    string Password,
    string LastName,
    string FirstName,
    string MiddleName,
    string PhoneNumber,
    string Email,
    Guid RoleId)
    : IValidation;