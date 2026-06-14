using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.DeleteCustomerCommand;

public sealed class DeleteCustomerCommandHandler(
    ICustomersRepository repository,
    IOrdersReadRepository ordersReadRepository)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customerIdResult = CustomerId.Create(command.Id);

        if (customerIdResult.IsFailure)
            return customerIdResult.Error.ToErrors();

        var customerId = customerIdResult.Value;

        var customer = await repository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            return CommonErrors.NotFound(
                    "customer.not.found",
                    "Клиент не найден")
                .ToErrors();
        }

        var hasOrdersResult = await ordersReadRepository.CustomerHasOrdersAsync(
            command.Id,
            cancellationToken);

        if (hasOrdersResult.IsFailure)
            return hasOrdersResult.Error;

        if (hasOrdersResult.Value)
        {
            return CommonErrors.Validation(
                    "customer.delete.forbidden.has.orders",
                    "Нельзя удалить клиента, у которого есть заказы")
                .ToErrors();
        }

        var deleteResult = customer.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Errors>();
    }
}