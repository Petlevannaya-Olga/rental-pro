using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class PaymentMethodsRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<PaymentMethodsRepository> logger)
    : IPaymentMethodsRepository
{
    public async Task<Result<PaymentMethod, Error>> AddAsync(
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.PaymentMethods.AddAsync(
                paymentMethod,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add payment method. Name = {Name}. Error = {Error}",
                    paymentMethod.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return paymentMethod;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Payment method creation operation was cancelled. Name = {Name}",
                paymentMethod.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.payment.method.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add payment method. Name = {Name}",
                paymentMethod.Name.Value);

            return CommonErrors.Db(
                "add.payment.method.to.db.exception",
                $"Failed to add payment method '{paymentMethod.Name.Value}'");
        }
    }

    public async Task<Result<List<PaymentMethod>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.PaymentMethods
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Payment methods retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.payment.methods.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve payment methods");

            return CommonErrors.Db(
                "get.payment.methods.from.db.exception",
                "Failed to retrieve payment methods");
        }
    }

    public async Task<Result<PaymentMethod?, Error>> GetByAsync(
        Expression<Func<PaymentMethod, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.PaymentMethods
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Payment method retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.payment.method.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve payment method");

            return CommonErrors.Db(
                "get.payment.method.from.db.exception",
                "Failed to retrieve payment method");
        }
    }
}