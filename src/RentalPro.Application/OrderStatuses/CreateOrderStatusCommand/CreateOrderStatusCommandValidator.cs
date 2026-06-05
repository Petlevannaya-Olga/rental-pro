using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.OrderStatuses.CreateOrderStatusCommand;

public class CreateOrderStatusCommandValidator: AbstractValidator<CreateOrderStatusCommand>
{
    public CreateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(OrderStatus.Create);
    }
}