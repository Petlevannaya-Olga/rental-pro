using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentTypes.CreatePaymentTypeCommand;

public sealed class CreatePaymentTypeCommandValidator 
    : AbstractValidator<CreatePaymentTypeCommand>
{
    public CreatePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(PaymentTypeName.Create);
    }
}