using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.UpdateOrderCommand;

public sealed class UpdateOrderCommandHandler(
    IOrdersRepository repository,
    ITransactionManager transactionManager)
    : ICommandHandler<UpdateOrderCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(command.Id).Value;

        var orderResult = await repository.GetByAsync(
            x => x.Id == orderId,
            cancellationToken);

        if (orderResult.IsFailure)
            return orderResult.Error.ToErrors();

        if (orderResult.Value is null)
        {
            return new Errors(
            [
                new Error(
                    "order.not.found",
                    "Заказ не найден",
                    ErrorType.NOT_FOUND)
            ]);
        }

        var order = orderResult.Value;

        var changeStatusResult = order.ChangeStatus(
            command.StatusId);

        if (changeStatusResult.IsFailure)
            return changeStatusResult.Error.ToErrors();

        var updateCommentResult = order.UpdateComment(
            command.Comment);

        if (updateCommentResult.IsFailure)
            return updateCommentResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }
}