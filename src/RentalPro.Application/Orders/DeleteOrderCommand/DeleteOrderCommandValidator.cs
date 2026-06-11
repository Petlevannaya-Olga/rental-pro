using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.DeleteOrderCommand;

public sealed class DeleteOrderCommandValidator
    : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(OrderId.Create);
    }
}