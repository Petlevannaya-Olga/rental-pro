using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.DeleteOrderCommand;

public sealed class DeleteOrderCommandHandler(
    IOrdersRepository repository,
    ITransactionManager transactionManager)
    : ICommandHandler<DeleteOrderCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteOrderCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var orderId = OrderId.Create(command.Id).Value;

        var orderResult = await repository.GetByAsync(
            x => x.Id == orderId,
            cancellationToken);

        if (orderResult.IsFailure)
        {
            transaction.Rollback();
            return orderResult.Error.ToErrors();
        }

        if (orderResult.Value is null)
        {
            transaction.Rollback();

            return new Errors(
            [
                new Error(
                    "order.not.found",
                    "Заказ не найден",
                    ErrorType.NOT_FOUND)
            ]);
        }

        var order = orderResult.Value;

        var itemsResult = await repository.GetItemsAsync(
            order.Id,
            cancellationToken);

        if (itemsResult.IsFailure)
        {
            transaction.Rollback();
            return itemsResult.Error.ToErrors();
        }

        foreach (var item in itemsResult.Value)
        {
            var deleteItemResult = item.Delete();

            if (deleteItemResult.IsFailure)
            {
                transaction.Rollback();
                return deleteItemResult.Error.ToErrors();
            }
        }

        var deleteOrderResult = order.Delete();

        if (deleteOrderResult.IsFailure)
        {
            transaction.Rollback();
            return deleteOrderResult.Error.ToErrors();
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