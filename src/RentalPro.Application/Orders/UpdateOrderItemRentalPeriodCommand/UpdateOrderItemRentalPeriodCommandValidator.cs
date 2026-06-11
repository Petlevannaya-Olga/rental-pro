using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.UpdateOrderItemRentalPeriodCommand;

public sealed class UpdateOrderItemRentalPeriodCommandValidator
    : AbstractValidator<UpdateOrderItemRentalPeriodCommand>
{
    public UpdateOrderItemRentalPeriodCommandValidator()
    {
        RuleFor(x => x.OrderItemId)
            .MustBeValueObject(OrderItemId.Create);

        RuleFor(x => x)
            .MustBeValueObject(command =>
                RentalPeriod.Create(
                    command.StartDate,
                    command.PlannedReturnDate));
    }
}