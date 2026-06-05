using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class OrderStatusesRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<OrderStatusesRepository> logger)
    : IOrderStatusesRepository
{
    public async Task<Result<OrderStatus, Error>> AddAsync(
        OrderStatus orderStatus,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.OrderStatuses.AddAsync(
                orderStatus,
                cancellationToken);

            var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
            {
                logger.LogError(
                    "Failed to add order status. Name = {Name}. Error = {Error}",
                    orderStatus.Name.Value,
                    saveResult.Error.Message);

                return saveResult.Error;
            }

            return orderStatus;
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Order  status creation operation was cancelled. Name = {Name}",
                orderStatus.Name.Value);

            return CommonErrors.OperationCancelled(
                "add.order.status.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add order status. Name = {Name}",
                orderStatus.Name.Value);

            return CommonErrors.Db(
                "add.order.status.to.db.exception",
                $"Failed to add order status '{orderStatus.Name.Value}'");
        }
    }

    public async Task<Result<List<OrderStatus>, Error>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.OrderStatuses
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Order status retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.order.statuses.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve order statuses");

            return CommonErrors.Db(
                "get.order.statuses.from.db.exception",
                "Failed to retrieve order statuses");
        }
    }

    public async Task<Result<OrderStatus?, Error>> GetByAsync(
        Expression<Func<OrderStatus, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.OrderStatuses
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Order status retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.order.status.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve order status");

            return CommonErrors.Db(
                "get.order.status.from.db.exception",
                "Failed to retrieve order status");
        }
    }
}