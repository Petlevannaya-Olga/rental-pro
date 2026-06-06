using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ToolsRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<ToolsRepository> logger)
    : IToolsRepository
{
    public async Task<Result<Tool?, Error>> GetByAsync(
        Expression<Func<Tool, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Tools
                .Include(x => x.Category)
                .Include(x => x.Manufacturer)
                .Include(x => x.Status)
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Tool retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.tool.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tool");

            return CommonErrors.Db(
                "get.tool.from.db.exception",
                "Failed to retrieve tool");
        }
    }

    public async Task<Result<Tool, Error>> AddAsync(
        Tool tool,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Tools.AddAsync(
                tool,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(
                cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add tool. Name = {Name}. InventoryNumber = {InventoryNumber}. SerialNumber = {SerialNumber}. Error = {Error}",
                    tool.Name,
                    tool.InventoryNumber.Value,
                    tool.SerialNumber.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return tool;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Tool creation operation was cancelled. Name = {Name}",
                tool.Name);

            return CommonErrors.OperationCancelled(
                "add.tool.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add tool. Name = {Name}. InventoryNumber = {InventoryNumber}. SerialNumber = {SerialNumber}",
                tool.Name,
                tool.InventoryNumber.Value,
                tool.SerialNumber.Value);

            return CommonErrors.Db(
                "add.tool.to.db.exception",
                $"Failed to add tool '{tool.Name}'");
        }
    }
}