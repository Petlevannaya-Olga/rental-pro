using CSharpFunctionalExtensions;
using RentalPro.Contracts.Users;
using RentalPro.Domain.Roles;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.GetUsersQuery;

public sealed class GetUsersQueryHandler(IUsersReadRepository userRepository)
    : IQueryHandler<PagedResult<UserDto>, GetUsersQuery>
{
    public async Task<Result<PagedResult<UserDto>, Errors>> Handle(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        RoleId? roleId = null;

        if (query.RoleId.HasValue)
        {
            var roleIdResult = RoleId.Create(query.RoleId.Value);

            if (roleIdResult.IsFailure)
                return roleIdResult.Error.ToErrors();

            roleId = roleIdResult.Value;
        }

        var usersResult = await userRepository.GetPagedAsync(
            query.Search,
            roleId,
            query.IsActive,
            query.CreatedFrom,
            query.CreatedTo,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);

        if (usersResult.IsFailure)
            return usersResult.Error;

        return usersResult.Value;
    }
}