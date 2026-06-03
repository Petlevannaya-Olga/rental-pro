using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.UpdateUserCommand;

public sealed record UpdateUserCommand(
    Guid Id,
    string LastName,
    string FirstName,
    string MiddleName,
    string Login,
    string Email,
    string PhoneNumber,
    Guid RoleId) : IValidation;