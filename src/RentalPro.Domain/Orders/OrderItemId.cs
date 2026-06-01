using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public readonly record struct OrderItemId
{
    public Guid Value { get; }

    private OrderItemId(Guid value)
    {
        Value = value;
    }

    public static OrderItemId NewId()
    {
        return new OrderItemId(Guid.NewGuid());
    }

    public static Result<OrderItemId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Order item id cannot be empty");
        }

        return new OrderItemId(value);
    }
    
    public static OrderItemId Restore(Guid value)
    {
        return new OrderItemId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}