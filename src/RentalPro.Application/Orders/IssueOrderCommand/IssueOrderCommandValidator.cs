using FluentValidation;
using RentalPro.Domain.Orders;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.IssueOrderCommand;

public sealed class IssueOrderCommandValidator
    : AbstractValidator<IssueOrderCommand>
{
    public IssueOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .MustBeValueObject(OrderId.Create);
    }
}