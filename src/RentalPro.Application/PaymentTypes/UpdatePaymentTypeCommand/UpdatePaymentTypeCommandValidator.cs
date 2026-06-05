using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentTypes.UpdatePaymentTypeCommand;

public sealed class UpdatePaymentTypeCommandValidator 
    : AbstractValidator<UpdatePaymentTypeCommand>
{
    public UpdatePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(PaymentTypeId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(PaymentTypeName.Create);
    }
}