using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class OrderStatus : AuditableEntity<OrderStatusId>
{
    private OrderStatus(OrderStatusName name)
        : base(OrderStatusId.NewId())
    {
        Name = name;
    }

    public OrderStatusName Name { get; private set; }

    public static Result<OrderStatus, Error> Create(string name)
    {
        var nameResult = OrderStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new OrderStatus(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = OrderStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(OrderStatus));
    }
}