using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.UpdateCustomerCommand;

public sealed class UpdateCustomerCommandHandler(
    ICustomersRepository repository)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdateCustomerCommand command,
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

        var addressResult = CreateAddress(command);

        if (addressResult.IsFailure)
            return addressResult.Error.ToErrors();

        var updateResult = customer.Update(
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.PhoneNumber,
            command.Email,
            command.PassportSeries,
            command.PassportNumber,
            addressResult.Value);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        await repository.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Errors>();
    }

    private static Result<Address, Error> CreateAddress(
        UpdateCustomerCommand command)
    {
        return Address.Create(
            command.PostalCode,
            command.Region,
            command.City,
            command.Street,
            command.House,
            command.Building,
            command.Apartment);
    }
}