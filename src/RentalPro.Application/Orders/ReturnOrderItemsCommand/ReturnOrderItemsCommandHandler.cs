using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ReturnOrderItemsCommand;

public sealed class ReturnOrderItemsHandler(
    IOrdersRepository ordersRepository,
    IToolsRepository toolsRepository,
    IToolStatusesRepository toolStatusesRepository,
    IOrdersReadRepository ordersReadRepository,
    IOrderStatusesRepository orderStatusesRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<ReturnOrderItemsCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        ReturnOrderItemsCommand command,
        CancellationToken cancellationToken = default)
    {
        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var orderIdResult = OrderId.Create(command.OrderId);

        if (orderIdResult.IsFailure)
        {
            transaction.Rollback();
            return orderIdResult.Error.ToErrors();
        }

        var orderDetailsResult = await ordersReadRepository.GetByIdAsync(
            orderIdResult.Value,
            cancellationToken);

        if (orderDetailsResult.IsFailure)
        {
            transaction.Rollback();
            return orderDetailsResult.Error;
        }

        var orderDetails = orderDetailsResult.Value;

        if (orderDetails.StatusName != "Выполняется")
        {
            transaction.Rollback();

            return CommonErrors.Validation(
                    "order.status",
                    "Возврат можно оформить только для выполняемого заказа")
                .ToErrors();
        }

        var availableStatusNameResult = ToolStatusName.Create("Доступен");

        if (availableStatusNameResult.IsFailure)
        {
            transaction.Rollback();
            return availableStatusNameResult.Error.ToErrors();
        }

        var availableStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Name == availableStatusNameResult.Value,
            cancellationToken);

        if (availableStatusResult.IsFailure)
        {
            transaction.Rollback();
            return availableStatusResult.Error.ToErrors();
        }

        var maintenanceStatusNameResult = ToolStatusName.Create("На ремонте");

        if (maintenanceStatusNameResult.IsFailure)
        {
            transaction.Rollback();
            return maintenanceStatusNameResult.Error.ToErrors();
        }

        var maintenanceStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Name == maintenanceStatusNameResult.Value,
            cancellationToken);

        if (maintenanceStatusResult.IsFailure)
        {
            transaction.Rollback();
            return maintenanceStatusResult.Error.ToErrors();
        }

        var needMaintenance = !string.IsNullOrWhiteSpace(command.DamageComment);

        foreach (var itemId in command.OrderItemIds)
        {
            var orderItemIdResult = OrderItemId.Create(itemId);

            if (orderItemIdResult.IsFailure)
            {
                transaction.Rollback();
                return orderItemIdResult.Error.ToErrors();
            }

            var orderItemResult = await ordersRepository.GetItemByAsync(
                x => x.Id == orderItemIdResult.Value &&
                     x.OrderId == orderIdResult.Value,
                cancellationToken);

            if (orderItemResult.IsFailure)
            {
                transaction.Rollback();
                return orderItemResult.Error.ToErrors();
            }

            var orderItem = orderItemResult.Value;

            var returnResult = orderItem.Return(
                command.ActualReturnedDate,
                command.ReturnCondition,
                command.DamageComment);

            if (returnResult.IsFailure)
            {
                transaction.Rollback();
                return returnResult.Error.ToErrors();
            }

            var toolResult = await toolsRepository.GetByAsync(
                x => x.Id == orderItem.ToolId,
                cancellationToken);

            if (toolResult.IsFailure)
            {
                transaction.Rollback();
                return toolResult.Error.ToErrors();
            }

            var tool = toolResult.Value;

            var targetStatusId = needMaintenance
                ? maintenanceStatusResult.Value.Id.Value
                : availableStatusResult.Value.Id.Value;

            var changeToolStatusResult = tool.ChangeStatus(targetStatusId);

            if (changeToolStatusResult.IsFailure)
            {
                transaction.Rollback();
                return changeToolStatusResult.Error.ToErrors();
            }
        }
        
        var hasNotReturnedItems = orderDetails.Items
            .Where(x => !command.OrderItemIds.Contains(x.Id))
            .Any();

        if (!hasNotReturnedItems)
        {
            var completedStatusNameResult = OrderStatusName.Create("Завершен");

            if (completedStatusNameResult.IsFailure)
            {
                transaction.Rollback();
                return completedStatusNameResult.Error.ToErrors();
            }

            var completedStatusResult = await orderStatusesRepository.GetByAsync(
                x => x.Name == completedStatusNameResult.Value,
                cancellationToken);

            if (completedStatusResult.IsFailure)
            {
                transaction.Rollback();
                return completedStatusResult.Error.ToErrors();
            }

            var orderResult = await ordersRepository.GetByAsync(
                x => x.Id == orderIdResult.Value,
                cancellationToken);

            if (orderResult.IsFailure)
            {
                transaction.Rollback();
                return orderResult.Error.ToErrors();
            }

            var order = orderResult.Value;

            var changeOrderStatusResult = order.ChangeStatus(
                completedStatusResult.Value.Id.Value);

            if (changeOrderStatusResult.IsFailure)
            {
                transaction.Rollback();
                return changeOrderStatusResult.Error.ToErrors();
            }
        }

        var notReturnedItemsLeft = orderDetails.Items
            .Where(x => !command.OrderItemIds.Contains(x.Id))
            .Any(x => x.ActualReturnedDate is null);
        
        if (!notReturnedItemsLeft)
        {
            var completedStatusNameResult = OrderStatusName.Create("Ожидает возврата залога");

            if (completedStatusNameResult.IsFailure)
            {
                transaction.Rollback();
                return completedStatusNameResult.Error.ToErrors();
            }

            var completedStatusResult = await orderStatusesRepository.GetByAsync(
                x => x.Name == completedStatusNameResult.Value,
                cancellationToken);

            if (completedStatusResult.IsFailure)
            {
                transaction.Rollback();
                return completedStatusResult.Error.ToErrors();
            }

            var orderResult = await ordersRepository.GetByAsync(
                x => x.Id == orderIdResult.Value,
                cancellationToken);

            if (orderResult.IsFailure)
            {
                transaction.Rollback();
                return orderResult.Error.ToErrors();
            }

            var order = orderResult.Value;

            var changeOrderStatusResult = order.ChangeStatus(
                completedStatusResult.Value.Id.Value);

            if (changeOrderStatusResult.IsFailure)
            {
                transaction.Rollback();
                return changeOrderStatusResult.Error.ToErrors();
            }
        }
        
        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }
}