using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.GetUsersQuery;

public sealed record GetUsersQuery(
    string? Search,
    Guid? RoleId,
    bool? IsActive,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    string? SortBy,
    bool Descending,
    int Page = 1,
    int PageSize = 10) : IQuery;