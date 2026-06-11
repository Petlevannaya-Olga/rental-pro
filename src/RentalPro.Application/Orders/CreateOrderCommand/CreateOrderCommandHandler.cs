using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CreateOrderCommand;

public sealed class CreateOrderCommandHandler(
    IOrdersRepository repository,
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

        var addOrderResult = await repository.AddAsync(
            order,
            cancellationToken);

        if (addOrderResult.IsFailure)
        {
            transaction.Rollback();
            return addOrderResult.Error.ToErrors();
        }

        foreach (var item in command.Items)
        {
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

            var addItemResult = await repository.AddItemAsync(
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