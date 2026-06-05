using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    public async Task<UnitResult<Error>> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (DbUpdateException e)
        {
            var sqlException = GetSqlException(e);

            if (sqlException?.Number is 2601 or 2627)
            {
                logger.LogError(
                    e,
                    "Unique constraint violation while saving changes");

                return CommonErrors.Conflict(
                    "db.unique.constraint.violation",
                    "Запись с такими данными уже существует");
            }

            logger.LogError(
                e,
                "Database update error while saving changes");

            return CommonErrors.Db(
                "db.update.exception",
                "Не удалось сохранить изменения в БД");
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning(
                e,
                "Save changes operation was cancelled");

            return CommonErrors.OperationCancelled(
                "save.changes.operation.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to save changes to database");

            return CommonErrors.Failure(
                "db.save.changes",
                "Не удалось сохранить изменения в БД");
        }
    }

    private static SqlException? GetSqlException(Exception exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is SqlException sqlException)
                return sqlException;

            current = current.InnerException;
        }

        return null;
    }
}