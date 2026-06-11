using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.UpdateOrderItemRentalPeriodCommand;

public sealed class UpdateOrderItemRentalPeriodCommandHandler(
    IOrdersRepository repository,
    ITransactionManager transactionManager)
    : ICommandHandler<UpdateOrderItemRentalPeriodCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateOrderItemRentalPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var orderItemId = OrderItemId.Create(
            command.OrderItemId).Value;

        var itemResult = await repository.GetItemByAsync(
            x => x.Id == orderItemId,
            cancellationToken);

        if (itemResult.IsFailure)
            return itemResult.Error.ToErrors();

        if (itemResult.Value is null)
        {
            return new Errors(
            [
                new Error(
                    "order.item.not.found",
                    "Позиция заказа не найдена",
                    ErrorType.NOT_FOUND)
            ]);
        }

        var orderItem = itemResult.Value;

        var updateResult = orderItem.UpdateRentalPeriod(
            command.StartDate,
            command.PlannedReturnDate);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }
}