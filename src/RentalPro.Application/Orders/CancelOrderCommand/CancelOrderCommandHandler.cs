using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CancelOrderCommand;

public sealed class CancelOrderCommandHandler(
    IOrdersRepository ordersRepository,
    IOrderStatusesRepository orderStatusesRepository,
    IToolsRepository toolsRepository,
    IToolStatusesRepository toolStatusesRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(command.OrderId).Value;

        var orderResult = await ordersRepository.GetByAsync(
            order => order.Id == orderId,
            cancellationToken);

        if (orderResult.IsFailure)
            return orderResult.Error.ToErrors();

        var order = orderResult.Value;

        if (order is null)
        {
            return CommonErrors.NotFound(
                    "order.not.found",
                    "Заказ не найден",
                    command.OrderId)
                .ToErrors();
        }

        var confirmedStatusNameResult = OrderStatusName.Create("Подтвержден");

        if (confirmedStatusNameResult.IsFailure)
            return confirmedStatusNameResult.Error.ToErrors();

        var confirmedStatusResult = await orderStatusesRepository.GetByAsync(
            status => status.Name == confirmedStatusNameResult.Value,
            cancellationToken);

        if (confirmedStatusResult.IsFailure)
            return confirmedStatusResult.Error.ToErrors();

        var confirmedStatus = confirmedStatusResult.Value;

        if (confirmedStatus is null)
        {
            return CommonErrors.NotFound(
                    "order.status.confirmed.not.found",
                    "Статус заказа \"Подтвержден\" не найден")
                .ToErrors();
        }

        var cancelledStatusNameResult = OrderStatusName.Create("Отменен");

        if (cancelledStatusNameResult.IsFailure)
            return cancelledStatusNameResult.Error.ToErrors();

        var cancelledStatusResult = await orderStatusesRepository.GetByAsync(
            status => status.Name == cancelledStatusNameResult.Value,
            cancellationToken);

        if (cancelledStatusResult.IsFailure)
            return cancelledStatusResult.Error.ToErrors();

        var cancelledStatus = cancelledStatusResult.Value;

        if (cancelledStatus is null)
        {
            return CommonErrors.NotFound(
                    "order.status.cancelled.not.found",
                    "Статус заказа \"Отменен\" не найден")
                .ToErrors();
        }

        if (order.StatusId != confirmedStatus.Id)
        {
            return CommonErrors.Validation(
                    "order.cannot.be.cancelled",
                    "Отменить можно только подтвержденный заказ")
                .ToErrors();
        }

        var availableToolStatusNameResult = ToolStatusName.Create("Доступен");

        if (availableToolStatusNameResult.IsFailure)
            return availableToolStatusNameResult.Error.ToErrors();

        var availableToolStatusResult = await toolStatusesRepository.GetByAsync(
            status => status.Name == availableToolStatusNameResult.Value,
            cancellationToken);

        if (availableToolStatusResult.IsFailure)
            return availableToolStatusResult.Error.ToErrors();

        var availableToolStatus = availableToolStatusResult.Value;

        if (availableToolStatus is null)
        {
            return CommonErrors.NotFound(
                    "tool.status.available.not.found",
                    "Статус инструмента \"Доступен\" не найден")
                .ToErrors();
        }

        var orderItemsResult = await ordersRepository.GetItemsAsync(
            orderId,
            cancellationToken);

        if (orderItemsResult.IsFailure)
            return orderItemsResult.Error.ToErrors();

        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        foreach (var item in orderItemsResult.Value)
        {
            var toolResult = await toolsRepository.GetByAsync(
                tool => tool.Id == item.ToolId,
                cancellationToken);

            if (toolResult.IsFailure)
            {
                transaction.Rollback();
                return toolResult.Error.ToErrors();
            }

            var tool = toolResult.Value;

            if (tool is null)
            {
                transaction.Rollback();

                return CommonErrors.NotFound(
                        "tool.not.found",
                        "Инструмент заказа не найден",
                        item.ToolId.Value)
                    .ToErrors();
            }

            tool.ChangeStatus(availableToolStatus.Id.Value);
        }

        order.ChangeStatus(cancelledStatus.Id.Value);

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