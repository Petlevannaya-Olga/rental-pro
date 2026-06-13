using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.ReturnOrderItemsCommand;

public sealed class ReturnOrderItemsCommandValidator
    : AbstractValidator<ReturnOrderItemsCommand>
{
    public ReturnOrderItemsCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .MustBeValueObject(OrderId.Create);

        RuleFor(x => x.OrderItemIds)
            .NotEmpty()
            .WithMessage("Выберите инструменты для возврата");

        RuleForEach(x => x.OrderItemIds)
            .MustBeValueObject(OrderItemId.Create);
    }
}