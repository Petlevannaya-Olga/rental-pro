using FluentValidation;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Orders.CreateOrderCommand;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .MustBeValueObject(UserId.Create);

        RuleFor(x => x.CustomerId)
            .MustBeValueObject(CustomerId.Create);

        RuleFor(x => x.StatusId)
            .MustBeValueObject(OrderStatusId.Create);

        RuleFor(x => x.DepositTotal)
            .MustBeValueObject(Money.Create);

        RuleFor(x => x.Comment)
            .MustBeValueObject(Comment.Create);
        
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemCommandValidator());
    }
}

public sealed class CreateOrderItemCommandValidator
    : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(x => x.ToolId)
            .MustBeValueObject(ToolId.Create);

        RuleFor(x => x.RentalPricePerDay)
            .MustBeValueObject(Money.Create);

        RuleFor(x => x)
            .MustBeValueObject(item =>
                RentalPeriod.Create(
                    item.StartDate,
                    item.PlannedReturnDate));
    }
}