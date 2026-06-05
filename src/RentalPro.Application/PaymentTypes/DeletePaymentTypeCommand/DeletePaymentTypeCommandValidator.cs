using FluentValidation;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.PaymentTypes.DeletePaymentTypeCommand;

public sealed class DeletePaymentTypeCommandValidator
    : AbstractValidator<DeletePaymentTypeCommand>
{
    public DeletePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(PaymentTypeId.Create);
    }
}