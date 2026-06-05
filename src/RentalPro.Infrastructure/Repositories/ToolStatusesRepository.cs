using RentalPro.Application.Repositories;

using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ToolStatusesRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<ToolStatusesRepository> logger)
    : IToolStatusesRepository
{
    public async Task<Result<ToolStatus, Error>> AddAsync(
        ToolStatus toolStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.ToolStatuses.AddAsync(
                toolStatus,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(
                cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add tool status. Name = {Name}. Error = {Error}",
                    toolStatus.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return toolStatus;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Tool status creation operation was cancelled. Name = {Name}",
                toolStatus.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.tool.status.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add tool status. Name = {Name}",
                toolStatus.Name.Value);

            return CommonErrors.Db(
                "add.tool.status.to.db.exception",
                $"Failed to add tool status '{toolStatus.Name.Value}'");
        }
    }

    public async Task<Result<ToolStatus?, Error>> GetByAsync(
        Expression<Func<ToolStatus, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.ToolStatuses
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Tool status retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.tool.status.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tool status");

            return CommonErrors.Db(
                "get.tool.status.from.db.exception",
                "Failed to retrieve tool status");
        }
    }

    public async Task<Result<List<ToolStatus>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.ToolStatuses
                .OrderBy(x => x.Name.Value)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Tool statuses retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.tool.statuses.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tool statuses");

            return CommonErrors.Db(
                "get.tool.statuses.from.db.exception",
                "Failed to retrieve tool statuses");
        }
    }
}