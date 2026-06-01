using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Database;

public class TransactionManager(
    ApplicationDbContext dbContext,
    ILogger<TransactionManager> logger,
    ILoggerFactory loggerFactory) : ITransactionManager
{
    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var transactionScopeLogger = loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);
            return transactionScope;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Операция была отменена");
            return CommonErrors.OperationCancelled("begin.transaction.operation.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось создать транзакцию");
            return CommonErrors.Failure("db.transaction", "Не удалось создать транзакцию");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Операция была отменена");
            return CommonErrors.OperationCancelled("save.changes.operation.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось сохранить изменения в БД");
            return CommonErrors.Failure("db.save.changes", "Не удалось сохранить изменения в БД");
        }
    }
}