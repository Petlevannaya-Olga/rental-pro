using RentalPro.Application.Repositories;
using RentalPro.Domain.Tools;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ToolCategoriesRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<ToolCategoriesRepository> logger)
    : IToolCategoriesRepository
{
    public async Task<Result<ToolCategory, Error>> AddAsync(
        ToolCategory toolCategory,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.ToolCategories.AddAsync(
                toolCategory,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(
                cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add tool category. Name = {Name}. Error = {Error}",
                    toolCategory.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return toolCategory;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Tool category creation operation was cancelled. Name = {Name}",
                toolCategory.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.tool.category.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add tool category. Name = {Name}",
                toolCategory.Name.Value);

            return CommonErrors.Db(
                "add.tool.category.to.db.exception",
                $"Failed to add tool category '{toolCategory.Name.Value}'");
        }
    }

    public async Task<Result<ToolCategory?, Error>> GetByAsync(
        Expression<Func<ToolCategory, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.ToolCategories
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Tool category retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.tool.category.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tool category");

            return CommonErrors.Db(
                "get.tool.category.from.db.exception",
                "Failed to retrieve tool category");
        }
    }

    public async Task<Result<List<ToolCategory>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.ToolCategories
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Tool categories retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.tool.categories.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tool categories");

            return CommonErrors.Db(
                "get.tool.categories.from.db.exception",
                "Failed to retrieve tool categories");
        }
    }
}