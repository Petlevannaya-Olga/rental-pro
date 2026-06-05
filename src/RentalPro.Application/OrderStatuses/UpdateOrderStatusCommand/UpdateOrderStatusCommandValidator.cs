using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.OrderStatuses.UpdateOrderStatusCommand;

public sealed class UpdateOrderStatusCommandValidator
    : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(OrderStatusId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(OrderStatusName.Create);
    }
}