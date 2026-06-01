using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application;
using RentalPro.Application.Database;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class UserRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<UserRepository> logger)
    : IUserRepository
{
    public async Task<Result<User, Error>> AddAsync(
        User user,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Users.AddAsync(
                user,
                cancellationToken);

            await transactionManager.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlException)
        {
            // SQL Server unique constraint violation:
            // 2601 - Cannot insert duplicate key row in object with unique index
            // 2627 - Violation of UNIQUE KEY constraint
            if (sqlException.Number is 2601 or 2627)
            {
                logger.LogError(
                    e,
                    "User creation failed because user already exists. Login = {Login}, Email = {Email}, PhoneNumber = {PhoneNumber}",
                    user.Login.Value,
                    user.Email.Value,
                    user.PhoneNumber.Value);

                return CommonErrors.Conflict(
                    "user.is.conflict",
                    "User already exists");
            }

            logger.LogError(
                e,
                "Failed to add user. Login = {Login}, Email = {Email}, PhoneNumber = {PhoneNumber}",
                user.Login.Value,
                user.Email.Value,
                user.PhoneNumber.Value);

            return CommonErrors.Db(
                "add.user.to.db.exception",
                $"Failed to add user '{user.Login.Value}'");
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "User creation operation was cancelled. Login = {Login}",
                user.Login.Value);

            return CommonErrors.OperationCancelled(
                "add.user.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add user. Login = {Login}, Email = {Email}, PhoneNumber = {PhoneNumber}",
                user.Login.Value,
                user.Email.Value,
                user.PhoneNumber.Value);

            return CommonErrors.Db(
                "add.user.to.db.exception",
                $"Failed to add user '{user.Login.Value}'");
        }

        return user;
    }

    public async Task<Result<User?, Error>> GetByAsync(
        Expression<Func<User, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext
                .Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("User retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.user.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve user");

            return CommonErrors.Db(
                "get.user.from.db.exception",
                "Failed to retrieve user");
        }
    }

    public async Task<Result<PagedResult<User>, Errors>> GetPagedAsync(
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

            var query = dbContext.Users
                .Include(x => x.Role)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim();

                query = query.Where(x =>
                    x.Login.Value.Contains(searchText) ||
                    x.Email.Value.Contains(searchText) ||
                    x.PhoneNumber.Value.Contains(searchText) ||
                    x.FullName.LastName.Contains(searchText) ||
                    x.FullName.FirstName.Contains(searchText) ||
                    x.FullName.MiddleName.Contains(searchText));
            }

            if (roleId is not null)
            {
                query = query.Where(x => x.RoleId == roleId);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (createdFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                var to = createdTo.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedAt < to);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, sortBy, descending);

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<User>(
                users,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Операция получения страницы пользователей была отменена");
            return CommonErrors.OperationCancelled("get.users.page.was.cancelled").ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении страницы пользователей");

            return CommonErrors.Db(
                "get.users.page.from.db.exception",
                "Ошибка при получении страницы пользователей").ToErrors();
        }
    }

    private static IQueryable<User> ApplySorting(
        IQueryable<User> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLower() switch
        {
            "fullname" => descending
                ? query.OrderByDescending(x => x.FullName.LastName)
                    .ThenByDescending(x => x.FullName.FirstName)
                    .ThenByDescending(x => x.FullName.MiddleName)
                : query.OrderBy(x => x.FullName.LastName)
                    .ThenBy(x => x.FullName.FirstName)
                    .ThenBy(x => x.FullName.MiddleName),

            "login" => descending
                ? query.OrderByDescending(x => x.Login.Value)
                : query.OrderBy(x => x.Login.Value),

            "email" => descending
                ? query.OrderByDescending(x => x.Email.Value)
                : query.OrderBy(x => x.Email.Value),

            "phonenumber" => descending
                ? query.OrderByDescending(x => x.PhoneNumber.Value)
                : query.OrderBy(x => x.PhoneNumber.Value),

            "status" => descending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),

            "createdat" => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}