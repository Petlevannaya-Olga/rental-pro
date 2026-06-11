using FluentValidation;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.UpdateOrderCommand;

public sealed class UpdateOrderCommandValidator
    : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(OrderId.Create);

        RuleFor(x => x.StatusId)
            .MustBeValueObject(OrderStatusId.Create);

        RuleFor(x => x.Comment)
            .MustBeValueObject(Comment.Create);
    }
}

public sealed class UpdateOrderItemCommandValidator
    : AbstractValidator<UpdateOrderItemDto>
{
    public UpdateOrderItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(OrderItemId.Create);

        RuleFor(x => x)
            .MustBeValueObject(item =>
                RentalPeriod.Create(
                    item.StartDate,
                    item.PlannedReturnDate));
    }
}