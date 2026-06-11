using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CompleteOrderCommand;

public sealed class CompleteOrderCommandHandler(
    IOrdersRepository repository,
    ITransactionManager transactionManager)
    : ICommandHandler<CompleteOrderCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CompleteOrderCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var orderId = OrderId.Create(command.OrderId).Value;

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

        if (itemsResult.Value.Count == 0)
        {
            transaction.Rollback();

            return new Errors(
            [
                new Error(
                    "order.items.not.found",
                    "Невозможно завершить заказ. У заказа отсутствуют позиции",
                    ErrorType.FAILURE)
            ]);
        }

        var hasUnreturnedTools = itemsResult.Value
            .Any(x => x.ActualReturnedDate is null);

        if (hasUnreturnedTools)
        {
            transaction.Rollback();

            return new Errors(
            [
                new Error(
                    "order.has.unreturned.tools",
                    "Невозможно завершить заказ. Не все инструменты возвращены",
                    ErrorType.FAILURE)
            ]);
        }

        var changeStatusResult = order.ChangeStatus(
            command.CompletedStatusId);

        if (changeStatusResult.IsFailure)
        {
            transaction.Rollback();
            return changeStatusResult.Error.ToErrors();
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