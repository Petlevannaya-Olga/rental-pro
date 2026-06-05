using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Users;
using RentalPro.Domain.Roles;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public class UsersReadRepository(ApplicationDbContext dbContext, ILogger<UsersReadRepository> logger)
    : IUsersReadRepository
{
    public async Task<Result<PagedResult<UserDto>, Errors>> GetPagedAsync(
        string? search,
        RoleId? roleId,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var searchText = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            var searchPattern = searchText is null
                ? null
                : $"%{searchText}%";

            var fullNameSearchPattern = searchText is null
                ? null
                : $"%{searchText.Replace(" ", "%")}%";

            var createdToExclusive = createdTo?.Date.AddDays(1);
            var orderBy = GetOrderBy(sortBy, descending);
            var offset = (page - 1) * pageSize;

            const string whereClause = """
                                       WHERE u.deleted_at IS NULL
                                         AND r.deleted_at IS NULL
                                         AND (
                                             @search IS NULL
                                             OR u.login LIKE @searchPattern
                                             OR u.email LIKE @searchPattern
                                             OR u.phone_number LIKE @searchPattern
                                             OR CONCAT_WS(' ',
                                                 u.last_name,
                                                 u.first_name,
                                                 u.middle_name
                                             ) LIKE @fullNameSearchPattern
                                         )
                                         AND (@roleId IS NULL OR u.role_id = @roleId)
                                         AND (@isActive IS NULL OR u.is_active = @isActive)
                                         AND (@createdFrom IS NULL OR u.created_at >= @createdFrom)
                                         AND (@createdTo IS NULL OR u.created_at < @createdTo)
                                       """;

            var countSql = $"""
                            SELECT COUNT(*) AS Value
                            FROM users u
                            INNER JOIN roles r ON r.id = u.role_id
                            {whereClause}
                            """;

            var totalCount = await dbContext.Database
                .SqlQueryRaw<int>(
                    countSql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@roleId", roleId?.Value),
                    CreateParameter("@isActive", isActive),
                    CreateParameter("@createdFrom", createdFrom),
                    CreateParameter("@createdTo", createdToExclusive))
                .SingleAsync(cancellationToken);

            var sql = $"""
                       SELECT
                           u.id AS Id,
                           CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS FullName,
                           u.login AS Login,
                           u.email AS Email,
                           u.phone_number AS PhoneNumber,
                           u.role_id AS RoleId,
                           r.name AS RoleName,
                           u.is_active AS IsActive,
                           u.created_at AS CreatedAt,
                           u.updated_at AS UpdatedAt
                       FROM users u
                       INNER JOIN roles r ON r.id = u.role_id
                       {whereClause}
                       {orderBy}
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
                       """;

            var users = await dbContext.Database
                .SqlQueryRaw<UserDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@roleId", roleId?.Value),
                    CreateParameter("@isActive", isActive),
                    CreateParameter("@createdFrom", createdFrom),
                    CreateParameter("@createdTo", createdToExclusive),
                    CreateParameter("@offset", offset),
                    CreateParameter("@pageSize", pageSize))
                .ToListAsync(cancellationToken);

            return new PagedResult<UserDto>(
                users,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("User page retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.users.page.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get users page");

            return CommonErrors.Db(
                "get.users.page.from.db.exception",
                "Failed to get users page").ToErrors();
        }
    }

    public async Task<Result<UserStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          COUNT(*) AS TotalCount,
                          SUM(CASE WHEN u.is_active = 1 THEN 1 ELSE 0 END) AS ActiveCount,
                          SUM(CASE WHEN r.name = N'Администратор' THEN 1 ELSE 0 END) AS AdminCount,
                          SUM(CASE WHEN u.is_active = 0 THEN 1 ELSE 0 END) AS BlockedCount
                      FROM users u
                      INNER JOIN roles r ON r.id = u.role_id
                      WHERE u.deleted_at IS NULL
                        AND r.deleted_at IS NULL
                      """;

            var stats = await dbContext.Database
                .SqlQueryRaw<UserStatsDto>(sql)
                .SingleAsync(cancellationToken);

            return stats;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.users.stats.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get users stats");

            return CommonErrors.Db(
                "get.users.stats.from.db.exception",
                "Failed to get users stats").ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<UserDto>, Errors>> GetForExportAsync(
        string? search,
        RoleId? roleId,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        try
        {
            var searchText = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            var searchPattern = searchText is null
                ? null
                : $"%{searchText}%";

            var fullNameSearchPattern = searchText is null
                ? null
                : $"%{searchText.Replace(" ", "%")}%";

            var createdToExclusive = createdTo?.Date.AddDays(1);
            var orderBy = GetOrderBy(sortBy, descending);

            const string whereClause = """
                                       WHERE u.deleted_at IS NULL
                                         AND r.deleted_at IS NULL
                                         AND (
                                             @search IS NULL
                                             OR u.login LIKE @searchPattern
                                             OR u.email LIKE @searchPattern
                                             OR u.phone_number LIKE @searchPattern
                                             OR CONCAT_WS(' ',
                                                 u.last_name,
                                                 u.first_name,
                                                 u.middle_name
                                             ) LIKE @fullNameSearchPattern
                                         )
                                         AND (@roleId IS NULL OR u.role_id = @roleId)
                                         AND (@isActive IS NULL OR u.is_active = @isActive)
                                         AND (@createdFrom IS NULL OR u.created_at >= @createdFrom)
                                         AND (@createdTo IS NULL OR u.created_at < @createdTo)
                                       """;

            var sql = $"""
                       SELECT
                           u.id AS Id,
                           CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS FullName,
                           u.login AS Login,
                           u.email AS Email,
                           u.phone_number AS PhoneNumber,
                           u.role_id AS RoleId,
                           r.name AS RoleName,
                           u.is_active AS IsActive,
                           u.created_at AS CreatedAt,
                           u.updated_at AS UpdatedAt
                       FROM users u
                       INNER JOIN roles r ON r.id = u.role_id
                       {whereClause}
                       {orderBy}
                       """;

            var users = await dbContext.Database
                .SqlQueryRaw<UserDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@roleId", roleId?.Value),
                    CreateParameter("@isActive", isActive),
                    CreateParameter("@createdFrom", createdFrom),
                    CreateParameter("@createdTo", createdToExclusive))
                .ToListAsync(cancellationToken);

            return users;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("export.users.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get users for export");

            return CommonErrors.Db(
                "export.users.from.db.exception",
                "Failed to get users for export").ToErrors();
        }
    }

    private static SqlParameter CreateParameter(string name, object? value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    private static string GetOrderBy(string? sortBy, bool descending)
    {
        var direction = descending ? "DESC" : "ASC";

        return sortBy?.ToLowerInvariant() switch
        {
            "fullname" => $"ORDER BY u.last_name {direction}, u.first_name {direction}, u.middle_name {direction}",
            "login" => $"ORDER BY u.login {direction}",
            "email" => $"ORDER BY u.email {direction}",
            "phonenumber" => $"ORDER BY u.phone_number {direction}",
            "status" => $"ORDER BY u.is_active {direction}",
            "createdat" => $"ORDER BY u.created_at {direction}",
            _ => "ORDER BY u.created_at DESC"
        };
    }
}