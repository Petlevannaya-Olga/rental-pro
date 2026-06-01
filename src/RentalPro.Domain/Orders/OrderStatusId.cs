using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public readonly record struct OrderStatusId
{
    public Guid Value { get; }

    private OrderStatusId(Guid value)
    {
        Value = value;
    }

    public static OrderStatusId NewId()
    {
        return new OrderStatusId(Guid.NewGuid());
    }

    public static Result<OrderStatusId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Order status id cannot be empty");
        }

        return new OrderStatusId(value);
    }
    
    public static OrderStatusId Restore(Guid value)
    {
        return new OrderStatusId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}