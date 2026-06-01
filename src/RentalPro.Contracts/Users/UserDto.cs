namespace RentalPro.Contracts.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Login { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public Guid RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
}