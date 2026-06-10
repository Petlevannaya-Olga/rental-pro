using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.DeleteCustomerCommand;

public sealed class DeleteCustomerCommandHandler(
    ICustomersRepository repository)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerId.Create(command.Id).Value;

        var customer = await repository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            return new Errors(
            [
                new Error(
                    "customer.not.found",
                    "Клиент не найден",
                    ErrorType.NOT_FOUND)
            ]);
        }

        var deleteResult = customer.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Errors>();
    }
}