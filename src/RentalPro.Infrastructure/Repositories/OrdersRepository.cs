using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class OrdersRepository(
    ApplicationDbContext dbContext,
    ITransactionManager transactionManager,
    ILogger<OrdersRepository> logger)
    : IOrdersRepository
{
    public async Task<Result<Order, Error>> AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Orders.AddAsync(
                order,
                cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlException)
        {
            if (sqlException.Number is 2601 or 2627)
            {
                logger.LogError(
                    e,
                    "Order creation failed because order already exists. CustomerId = {CustomerId}",
                    order.CustomerId.Value);

                return CommonErrors.Conflict(
                    "order.is.conflict",
                    "Order already exists");
            }

            logger.LogError(
                e,
                "Failed to add order. CustomerId = {CustomerId}, UserId = {UserId}",
                order.CustomerId.Value,
                order.UserId.Value);

            return CommonErrors.Db(
                "add.order.to.db.exception",
                "Failed to add order");
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Order creation operation was cancelled. CustomerId = {CustomerId}",
                order.CustomerId.Value);

            return CommonErrors.OperationCancelled(
                "add.order.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add order. CustomerId = {CustomerId}, UserId = {UserId}",
                order.CustomerId.Value,
                order.UserId.Value);

            return CommonErrors.Db(
                "add.order.to.db.exception",
                "Failed to add order");
        }

        return order;
    }

    public async Task<Result<Order?, Error>> GetByAsync(
        Expression<Func<Order, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext
                .Orders
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Order retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.order.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve order");

            return CommonErrors.Db(
                "get.order.from.db.exception",
                "Failed to retrieve order");
        }
    }

    public async Task<Result<OrderItem, Error>> AddItemAsync(
        OrderItem orderItem,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.OrderItems.AddAsync(
                orderItem,
                cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlException)
        {
            if (sqlException.Number is 2601 or 2627)
            {
                logger.LogError(
                    e,
                    "Order item creation failed because order item already exists. OrderId = {OrderId}, ToolId = {ToolId}",
                    orderItem.OrderId.Value,
                    orderItem.ToolId.Value);

                return CommonErrors.Conflict(
                    "order.item.is.conflict",
                    "Order item already exists");
            }

            logger.LogError(
                e,
                "Failed to add order item. OrderId = {OrderId}, ToolId = {ToolId}",
                orderItem.OrderId.Value,
                orderItem.ToolId.Value);

            return CommonErrors.Db(
                "add.order.item.to.db.exception",
                "Failed to add order item");
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "Order item creation operation was cancelled. OrderId = {OrderId}",
                orderItem.OrderId.Value);

            return CommonErrors.OperationCancelled(
                "add.order.item.was.cancelled");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to add order item. OrderId = {OrderId}, ToolId = {ToolId}",
                orderItem.OrderId.Value,
                orderItem.ToolId.Value);

            return CommonErrors.Db(
                "add.order.item.to.db.exception",
                "Failed to add order item");
        }

        return orderItem;
    }

    public async Task<Result<OrderItem?, Error>> GetItemByAsync(
        Expression<Func<OrderItem, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext
                .OrderItems
                .FirstOrDefaultAsync(
                    expression,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Order item retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                "get.order.item.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve order item");

            return CommonErrors.Db(
                "get.order.item.from.db.exception",
                "Failed to retrieve order item");
        }
    }

    public async Task<Result<IReadOnlyList<OrderItem>, Error>> GetItemsAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.OrderItems
                .Where(x => x.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogError(
                "Order items retrieval operation was cancelled. OrderId = {OrderId}",
                orderId.Value);

            return CommonErrors.OperationCancelled(
                "get.order.items.was.cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to retrieve order items. OrderId = {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                "get.order.items.from.db.exception",
                "Failed to retrieve order items");
        }
    }
}