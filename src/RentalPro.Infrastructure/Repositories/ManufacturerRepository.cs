using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ManufacturerRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<ManufacturerRepository> logger)
    : IManufacturerRepository
{
    public async Task<Result<Manufacturer, Error>> AddAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Manufacturers.AddAsync(
                manufacturer,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(
                cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add manufacturer. Name = {Name}. Error = {Error}",
                    manufacturer.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return manufacturer;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Manufacturer creation operation was cancelled. Name = {Name}",
                manufacturer.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.manufacturer.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add manufacturer. Name = {Name}",
                manufacturer.Name.Value);

            return CommonErrors.Db(
                "add.manufacturer.to.db.exception",
                $"Failed to add manufacturer '{manufacturer.Name.Value}'");
        }
    }

    public async Task<Result<Manufacturer?, Error>> GetByAsync(
        Expression<Func<Manufacturer, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Manufacturers
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Manufacturer retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.manufacturer.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve manufacturer");

            return CommonErrors.Db(
                "get.manufacturer.from.db.exception",
                "Failed to retrieve manufacturer");
        }
    }

    public async Task<Result<List<Manufacturer>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Manufacturers
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Manufacturers retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.manufacturers.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve manufacturers");

            return CommonErrors.Db(
                "get.manufacturers.from.db.exception",
                "Failed to retrieve manufacturers");
        }
    }
}