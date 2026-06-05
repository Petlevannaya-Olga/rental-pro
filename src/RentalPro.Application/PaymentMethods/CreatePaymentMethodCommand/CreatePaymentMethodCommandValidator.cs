using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentMethods.CreatePaymentMethodCommand;

public sealed class CreatePaymentMethodCommandValidator 
    : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(PaymentMethodName.Create);
    }
}