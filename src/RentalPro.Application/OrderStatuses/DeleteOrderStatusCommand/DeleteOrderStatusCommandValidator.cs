using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.OrderStatuses.DeleteOrderStatusCommand;

public sealed class DeleteOrderStatusCommandValidator
    : AbstractValidator<DeleteOrderStatusCommand>
{
    public DeleteOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(OrderStatusId.Create);
    }
}