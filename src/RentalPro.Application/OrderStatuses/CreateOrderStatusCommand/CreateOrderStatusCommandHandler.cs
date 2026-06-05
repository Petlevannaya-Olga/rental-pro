using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.CreateOrderStatusCommand;

public sealed class CreateOrderStatusCommandHandler(
    IOrderStatusesRepository repository,
    ILogger<CreateOrderStatusCommandHandler> logger)
    : ICommandHandler<CreateOrderStatusCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreateOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var status = OrderStatus.Create(command.Name);
        
        var addResult = await repository.AddAsync(
            status.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Order status '{OrderStatusName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}