namespace RentalPro.Contracts.Users;

public sealed class CreateUserRequest
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string MiddleName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
}