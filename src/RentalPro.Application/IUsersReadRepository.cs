using CSharpFunctionalExtensions;
using RentalPro.Contracts.Users;
using RentalPro.Domain.Roles;
using RentalPro.Shared;

namespace RentalPro.Application;

public interface IUsersReadRepository
{
    Task<Result<PagedResult<UserDto>, Errors>> GetPagedAsync(
        string? search,
        RoleId? roleId,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    
    Task<Result<UserStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);
}