using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class UsersRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<UsersRepository> logger)
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
                ? query.OrderByDescending(x => x.Login)
                : query.OrderBy(x => x.Login),

            "email" => descending
                ? query.OrderByDescending(x => x.Email)
                : query.OrderBy(x => x.Email),

            "phonenumber" => descending
                ? query.OrderByDescending(x => x.PhoneNumber)
                : query.OrderBy(x => x.PhoneNumber),

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