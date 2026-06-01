using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Orders;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Fines;

public sealed class Fine : AuditableEntity<FineId>
{
    private Fine(
        OrderItemId orderItemId,
        DateTime fineDate,
        Money amount,
        FineReason reason)
        : base(FineId.NewId())
    {
        OrderItemId = orderItemId;
        FineDate = fineDate;
        Amount = amount;
        Reason = reason;
    }

    public OrderItemId OrderItemId { get; private set; }

    public DateTime FineDate { get; private set; }

    public Money Amount { get; private set; }

    public FineReason Reason { get; private set; }

    public static Result<Fine, Error> Create(
        Guid orderItemId,
        DateTime fineDate,
        decimal amount,
        string reason)
    {
        var orderItemIdResult = OrderItemId.Create(orderItemId);

        if (orderItemIdResult.IsFailure)
            return orderItemIdResult.Error;

        var amountResult = Money.Create(amount);

        if (amountResult.IsFailure)
            return amountResult.Error;

        var reasonResult = FineReason.Create(reason);

        if (reasonResult.IsFailure)
            return reasonResult.Error;

        return new Fine(
            orderItemIdResult.Value,
            fineDate,
            amountResult.Value,
            reasonResult.Value);
    }

    public UnitResult<Error> Update(
        decimal amount,
        string reason)
    {
        var amountResult = Money.Create(amount);

        if (amountResult.IsFailure)
            return amountResult.Error;

        var reasonResult = FineReason.Create(reason);

        if (reasonResult.IsFailure)
            return reasonResult.Error;

        Amount = amountResult.Value;
        Reason = reasonResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Fine));
    }
}