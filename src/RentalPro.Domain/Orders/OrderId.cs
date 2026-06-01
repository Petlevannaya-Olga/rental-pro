using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public readonly record struct OrderId
{
    public Guid Value { get; }

    private OrderId(Guid value)
    {
        Value = value;
    }

    public static OrderId NewId()
    {
        return new OrderId(Guid.NewGuid());
    }

    public static Result<OrderId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Order id cannot be empty");
        }

        return new OrderId(value);
    }
    
    public static OrderId Restore(Guid value)
    {
        return new OrderId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}