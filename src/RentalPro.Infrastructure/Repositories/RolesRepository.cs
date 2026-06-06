using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Roles;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class RolesRepository(
    ApplicationDbContext dbContext,
    ITransactionManager  transactionManager,
    ILogger<RolesRepository> logger)
    : IRoleRepository
{
    public async Task<Result<Role?, Error>> GetByAsync(
        Expression<Func<Role, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext
                .Roles
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Role retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.role.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve role");

            return CommonErrors.Db(
                "get.role.from.db.exception",
                "Failed to retrieve role");
        }
    }
    
    public async Task<Result<List<Role>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Roles
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Role retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.roles.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve roles");

            return CommonErrors.Db(
                "get.roles.from.db.exception",
                "Failed to retrieve roles");
        }
    }

    public async Task<Result<Role, Error>> AddAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Roles.AddAsync(
                role,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(
                cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add role. Name = {Name}. Error = {Error}",
                    role.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return role;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Role creation operation was cancelled. Name = {Name}",
                role.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.role.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add role. Name = {Name}",
                role.Name.Value);

            return CommonErrors.Db(
                "add.role.to.db.exception",
                $"Failed to add role '{role.Name.Value}'");
        }
    }

}