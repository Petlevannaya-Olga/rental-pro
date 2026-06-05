using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class PaymentTypesRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<PaymentTypesRepository> logger)
    : IPaymentTypesRepository
{
    public async Task<Result<PaymentType, Error>> AddAsync(
        PaymentType paymentType,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.PaymentTypes.AddAsync(
                paymentType,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add payment type. Name = {Name}. Error = {Error}",
                    paymentType.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return paymentType;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Payment type creation operation was cancelled. Name = {Name}",
                paymentType.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.payment.type.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add payment type. Name = {Name}",
                paymentType.Name.Value);

            return CommonErrors.Db(
                "add.payment.type.to.db.exception",
                $"Failed to add payment type '{paymentType.Name.Value}'");
        }
    }

    public async Task<Result<List<PaymentType>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.PaymentTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Payment types retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.payment.types.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve payment types");

            return CommonErrors.Db(
                "get.payment.types.from.db.exception",
                "Failed to retrieve payment types");
        }
    }

    public async Task<Result<PaymentType?, Error>> GetByAsync(
        Expression<Func<PaymentType, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.PaymentTypes
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Payment type retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.payment.type.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve payment type");

            return CommonErrors.Db(
                "get.payment.type.from.db.exception",
                "Failed to retrieve payment method");
        }
    }
}