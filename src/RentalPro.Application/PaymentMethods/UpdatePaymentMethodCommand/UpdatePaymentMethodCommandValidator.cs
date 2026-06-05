using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentMethods.UpdatePaymentMethodCommand;

public sealed class UpdatePaymentMethodCommandValidator 
    : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(PaymentMethodId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(PaymentMethodName.Create);
    }
}