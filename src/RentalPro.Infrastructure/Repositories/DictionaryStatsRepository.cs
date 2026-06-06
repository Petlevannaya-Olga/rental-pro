using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class DictionaryStatsRepository(
    ApplicationDbContext dbContext,
    ILogger<DictionaryStatsRepository> logger)
    : IDictionaryStatsRepository
{
    public async Task<Result<DictionaryStatsDto, Error>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return new DictionaryStatsDto(
                PaymentMethods: await dbContext.PaymentMethods.CountAsync(cancellationToken),
                PaymentTypes: await dbContext.PaymentTypes.CountAsync(cancellationToken),
                OrderStatuses: await dbContext.OrderStatuses.CountAsync(cancellationToken),
                ToolCategories: await dbContext.ToolCategories.CountAsync(cancellationToken),
                ToolStatuses: await dbContext.ToolStatuses.CountAsync(cancellationToken),
                Roles: await dbContext.Roles.CountAsync(cancellationToken),
                Manufacturers: await dbContext.Manufacturers.CountAsync(cancellationToken),
                Suppliers: await dbContext.Suppliers.CountAsync(cancellationToken));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load dictionary stats");

            return CommonErrors.Db(
                "dictionary.stats.load.failed",
                "Не удалось загрузить статистику справочников");
        }
    }
}