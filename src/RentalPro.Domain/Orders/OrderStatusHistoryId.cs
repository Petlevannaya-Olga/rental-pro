using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public readonly record struct OrderStatusHistoryId
{
    public Guid Value { get; }

    private OrderStatusHistoryId(Guid value)
    {
        Value = value;
    }

    public static OrderStatusHistoryId NewId()
    {
        return new OrderStatusHistoryId(Guid.NewGuid());
    }

    public static Result<OrderStatusHistoryId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Order status history id cannot be empty");
        }

        return new OrderStatusHistoryId(value);
    }
    
    public static OrderStatusHistoryId Restore(Guid value)
    {
        return new OrderStatusHistoryId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}