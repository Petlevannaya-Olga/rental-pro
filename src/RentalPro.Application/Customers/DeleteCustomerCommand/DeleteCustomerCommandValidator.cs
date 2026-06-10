using FluentValidation;
using RentalPro.Domain.Customers;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Customers.DeleteCustomerCommand;

public sealed class DeleteCustomerCommandValidator
    : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(CustomerId.Create);
    }
}