using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.UpdateOrderStatusCommand;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderStatusesRepository repository,
    ITransactionManager transactionManager,
    ILogger<UpdateOrderStatusCommandHandler> logger)
    : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var orderStatusId = OrderStatusId.Restore(command.Id);

        var statusResult = await repository.GetByAsync(
            x => x.Id == orderStatusId,
            cancellationToken);

        if (statusResult.IsFailure)
            return statusResult.Error.ToErrors();

        if (statusResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "order.status.not.found",
                    $"Order status with id '{command.Id}' was not found")
                .ToErrors();
        }

        var updateResult = statusResult.Value.Update(command.Name);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Order status with id '{OrderStatusId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}