using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CreateOrderCommand;

public sealed class CreateOrderCommandHandler(
    IOrdersRepository ordersRepository,
    IToolsRepository toolsRepository,
    IToolStatusesRepository toolStatusesRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<Guid, CreateOrderCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var rentedStatusNameResult = ToolStatusName.Create("В аренде");

        if (rentedStatusNameResult.IsFailure)
        {
            transaction.Rollback();
            return rentedStatusNameResult.Error.ToErrors();
        }

        var rentedStatusResult = await toolStatusesRepository.GetByAsync(
            x => x.Name == rentedStatusNameResult.Value,
            cancellationToken);

        if (rentedStatusResult.IsFailure)
        {
            transaction.Rollback();
            return rentedStatusResult.Error.ToErrors();
        }

        var rentedStatus = rentedStatusResult.Value;

        var orderResult = Order.Create(
            command.UserId,
            command.CustomerId,
            command.OrderDate,
            command.StatusId,
            command.Comment);

        if (orderResult.IsFailure)
        {
            transaction.Rollback();
            return orderResult.Error.ToErrors();
        }

        var order = orderResult.Value;

        var addOrderResult = await ordersRepository.AddAsync(
            order,
            cancellationToken);

        if (addOrderResult.IsFailure)
        {
            transaction.Rollback();
            return addOrderResult.Error.ToErrors();
        }

        foreach (var item in command.Items)
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

            var changeStatusResult = tool.ChangeStatus(rentedStatus.Id.Value);

            if (changeStatusResult.IsFailure)
            {
                transaction.Rollback();
                return changeStatusResult.Error.ToErrors();
            }

            var orderItemResult = OrderItem.Create(
                order.Id.Value,
                item.ToolId,
                item.DepositAmount,
                item.RentalPricePerDay,
                item.StartDate,
                item.PlannedReturnDate);

            if (orderItemResult.IsFailure)
            {
                transaction.Rollback();
                return orderItemResult.Error.ToErrors();
            }

            var addItemResult = await ordersRepository.AddItemAsync(
                orderItemResult.Value,
                cancellationToken);

            if (addItemResult.IsFailure)
            {
                transaction.Rollback();
                return addItemResult.Error.ToErrors();
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

        return order.Id.Value;
    }
}