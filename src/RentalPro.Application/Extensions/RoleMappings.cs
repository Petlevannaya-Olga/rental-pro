using RentalPro.Contracts.Roles;
using RentalPro.Domain.Roles;

namespace RentalPro.Application.Extensions;

public static class RoleMappings
{
    public static RoleDto ToDto(this Role user)
    {
        return new RoleDto(user.Id.Value, user.Name.Value, user.CreatedAt, user.UpdatedAt);
    }
}