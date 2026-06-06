namespace RentalPro.Contracts.Roles;

public sealed record RoleDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);