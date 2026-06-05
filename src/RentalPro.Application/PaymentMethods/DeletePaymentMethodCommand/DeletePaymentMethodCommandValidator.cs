using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentMethods.DeletePaymentMethodCommand;

public sealed class DeletePaymentMethodCommandValidator 
    : AbstractValidator<DeletePaymentMethodCommand>
{
    public DeletePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(PaymentMethodId.Create);
    }
}