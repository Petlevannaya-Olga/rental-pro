namespace RentalPro.Contracts.Users;

public sealed record CreateUserDto(
    string Login,
    string Password,
    string LastName,
    string FirstName,
    string MiddleName,
    string PhoneNumber,
    string Email,
    Guid RoleId);