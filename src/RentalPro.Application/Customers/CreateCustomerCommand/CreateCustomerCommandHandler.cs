using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.CreateCustomerCommand;

public sealed class CreateCustomerCommandHandler(
    ICustomersRepository repository)
    : ICommandHandler<Guid, CreateCustomerCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var addressResult = CreateAddress(command);

        if (addressResult.IsFailure)
            return addressResult.Error.ToErrors();

        var customerResult = Customer.Create(
            command.LastName,
            command.FirstName,
            command.MiddleName,
            command.PhoneNumber,
            command.Email,
            command.PassportSeries,
            command.PassportNumber,
            addressResult.Value);

        if (customerResult.IsFailure)
            return customerResult.Error.ToErrors();

        await repository.AddAsync(
            customerResult.Value,
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return customerResult.Value.Id.Value;
    }

    private static Result<Address, Error> CreateAddress(
        CreateCustomerCommand command)
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