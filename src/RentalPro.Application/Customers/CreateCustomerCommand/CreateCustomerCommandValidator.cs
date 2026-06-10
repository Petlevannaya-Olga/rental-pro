using FluentValidation;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Customers.CreateCustomerCommand;

public sealed class CreateCustomerCommandValidator
    : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.LastName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(CreateCustomerCommand.LastName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid last name");

        RuleFor(x => x.FirstName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(CreateCustomerCommand.FirstName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid first name");

        RuleFor(x => x.MiddleName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(CreateCustomerCommand.MiddleName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid middle name");

        RuleFor(x => x.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create);

        RuleFor(x => x.PassportSeries)
            .MustBeValueObject(value =>
                PassportData.Create(
                    value,
                    "123456"));

        RuleFor(x => x.PassportNumber)
            .MustBeValueObject(value =>
                PassportData.Create(
                    "1234",
                    value));

        RuleFor(x => x)
            .MustBeValueObject(command =>
                Address.Create(
                    command.PostalCode,
                    command.Region,
                    command.City,
                    command.Street,
                    command.House,
                    command.Building,
                    command.Apartment));
    }
}