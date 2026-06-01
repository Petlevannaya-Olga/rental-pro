using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Database;

public class TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger) : ITransactionScope
{
    public UnitResult<Error> Commit()
    {
        try
        {
            transaction.Commit();
            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Операция была отменена");
            return CommonErrors.OperationCancelled("commit.operation.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось сохранить транзакцию");
            return UnitResult.Failure(CommonErrors.Failure(
                "transaction.commit.failed",
                "Не удалось сохранить транзакцию"));
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            transaction.Rollback();
            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Операция была отменена");
            return CommonErrors.OperationCancelled("rollback.operation.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось отменить транзакцию");
            return UnitResult.Failure(CommonErrors.Failure(
                "transaction.rollback.failed",
                "Не удалось отменить транзакцию"));
        }
    }

    public void Dispose() => transaction.Dispose();
}