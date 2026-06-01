using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application;
using RentalPro.Domain.Roles;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class RoleRepository(
    ApplicationDbContext dbContext,
    ILogger<RoleRepository> logger)
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
}