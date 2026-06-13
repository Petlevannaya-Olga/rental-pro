using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.IssueOrderCommand;

public sealed class IssueOrderHandler(
    IOrdersRepository ordersRepository,
    IToolsRepository toolsRepository,
    IOrderStatusesRepository orderStatusesRepository,
    IToolStatusesRepository toolStatusesRepository,
    IOrdersReadRepository ordersReadRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<IssueOrderCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        IssueOrderCommand command,
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

        if (orderDetails.StatusName != "Готов к выдаче")
        {
            transaction.Rollback();

            return CommonErrors.Validation(
                    "order.status",
                    "Заказ не готов к выдаче")
                .ToErrors();
        }

        if (orderDetails.TotalRemainingAmount > 0.01m)
        {
            transaction.Rollback();

            return CommonErrors.Validation(
                    "order.payment",
                    "Заказ нельзя выдать, пока он не оплачен полностью")
                .ToErrors();
        }

        var inProgressStatusNameResult = OrderStatusName.Create("Выполняется");

        if (inProgressStatusNameResult.IsFailure)
        {
            transaction.Rollback();
            return inProgressStatusNameResult.Error.ToErrors();
        }

        var inProgressStatusResult = await orderStatusesRepository.GetByAsync(
            x => x.Name == inProgressStatusNameResult.Value,
            cancellationToken);

        if (inProgressStatusResult.IsFailure)
        {
            transaction.Rollback();
            return inProgressStatusResult.Error.ToErrors();
        }

        var rentedToolStatusNameResult = ToolStatusName.Create("В аренде");

        if (rentedToolStatusNameResult.IsFailure)
        {
            transaction.Rollback();
            return rentedToolStatusNameResult.Error.ToErrors();
        }

        var rentedToolStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Name == rentedToolStatusNameResult.Value,
            cancellationToken);

        if (rentedToolStatusResult.IsFailure)
        {
            transaction.Rollback();
            return rentedToolStatusResult.Error.ToErrors();
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
            inProgressStatusResult.Value.Id.Value);

        if (changeOrderStatusResult.IsFailure)
        {
            transaction.Rollback();
            return changeOrderStatusResult.Error.ToErrors();
        }

        foreach (var item in orderDetails.Items)
        {
            var toolIdResult = ToolId.Create(item.ToolId);

            if (toolIdResult.IsFailure)
            {
                transaction.Rollback();
                return toolIdResult.Error.ToErrors();
            }

            var toolResult = await toolsRepository.GetByAsync(
                x => x.Id == toolIdResult.Value,
                cancellationToken);

            if (toolResult.IsFailure)
            {
                transaction.Rollback();
                return toolResult.Error.ToErrors();
            }

            var tool = toolResult.Value;

            var changeToolStatusResult = tool.ChangeStatus(
                rentedToolStatusResult.Value.Id.Value);

            if (changeToolStatusResult.IsFailure)
            {
                transaction.Rollback();
                return changeToolStatusResult.Error.ToErrors();
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