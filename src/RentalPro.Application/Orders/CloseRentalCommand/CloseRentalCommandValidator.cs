using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.CloseRentalCommand;

public sealed class CloseRentalCommandValidator
    : AbstractValidator<CloseRentalCommand>
{
    public CloseRentalCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .MustBeValueObject(OrderId.Create);

        RuleFor(x => x.PaymentMethodId)
            .MustBeValueObject(PaymentMethodId.Create);

        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("Дата операции обязательна");
    }
}