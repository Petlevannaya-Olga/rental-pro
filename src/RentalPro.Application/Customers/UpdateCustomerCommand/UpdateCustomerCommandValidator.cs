using FluentValidation;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Customers.UpdateCustomerCommand;

public sealed class UpdateCustomerCommandValidator
    : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(CustomerId.Create);

        RuleFor(x => x.LastName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(UpdateCustomerCommand.LastName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid last name");

        RuleFor(x => x.FirstName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(UpdateCustomerCommand.FirstName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid first name");

        RuleFor(x => x.MiddleName)
            .Must(value =>
                FullName.ValidatePart(
                        nameof(UpdateCustomerCommand.MiddleName),
                        value)
                    .IsSuccess)
            .WithMessage("Invalid middle name");

        RuleFor(x => x.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create);

        RuleFor(x => x)
            .MustBeValueObject(command =>
                PassportData.Create(
                    command.PassportSeries,
                    command.PassportNumber));

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