using RentalPro.Contracts.Users;
using RentalPro.Domain.Users;
using RentalPro.Shared;

namespace RentalPro.Application.Extensions;

public static class UserMappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id.Value,
            FullName = user.FullName.ToString(),
            Login = user.Login.Value,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber.Value,
            RoleId = user.RoleId.Value,
            RoleName = user.Role.Name.Value,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
    
    public static PagedResult<UserDto> ToDto(
        this PagedResult<User> result)
    {
        return new PagedResult<UserDto>(
            result.Items.Select(x => x.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public static List<UserDto> ToDtoList(this IEnumerable<User> users)
    {
        return users
            .Select(x => x.ToDto())
            .ToList();
    }
}