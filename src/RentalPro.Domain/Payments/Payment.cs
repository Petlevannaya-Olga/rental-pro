using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Orders;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public sealed class Payment : AuditableEntity<PaymentId>
{
    // EF Core
    private Payment()
        : base(PaymentId.NewId())
    {
    }
    
    private Payment(
        OrderId orderId,
        PaymentMethodId paymentMethodId,
        PaymentTypeId paymentTypeId,
        DateTime paymentDate,
        Money amount,
        Comment? comment)
        : base(PaymentId.NewId())
    {
        OrderId = orderId;
        PaymentMethodId = paymentMethodId;
        PaymentTypeId = paymentTypeId;
        PaymentDate = paymentDate;
        Amount = amount;
        Comment = comment;
    }

    public OrderId OrderId { get; private set; }

    public PaymentMethodId PaymentMethodId { get; private set; }

    public PaymentTypeId PaymentTypeId { get; private set; }

    public DateTime PaymentDate { get; private set; }

    public Money Amount { get; private set; }

    public Comment? Comment { get; private set; }

    public static Result<Payment, Error> Create(
        Guid orderId,
        Guid paymentMethodId,
        Guid paymentTypeId,
        Guid? fineId,
        DateTime paymentDate,
        decimal amount,
        string? comment)
    {
        var orderIdResult = OrderId.Create(orderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error;

        var paymentMethodIdResult = PaymentMethodId.Create(paymentMethodId);

        if (paymentMethodIdResult.IsFailure)
            return paymentMethodIdResult.Error;

        var paymentTypeIdResult = PaymentTypeId.Create(paymentTypeId);

        if (paymentTypeIdResult.IsFailure)
            return paymentTypeIdResult.Error;

        var amountResult = Money.Create(amount);

        if (amountResult.IsFailure)
            return amountResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new Payment(
            orderIdResult.Value,
            paymentMethodIdResult.Value,
            paymentTypeIdResult.Value,
            paymentDate,
            amountResult.Value,
            commentResult.Value);
    }

    public UnitResult<Error> Update(
        Guid paymentMethodId,
        Guid paymentTypeId,
        Guid? fineId,
        DateTime paymentDate,
        decimal amount,
        string? comment)
    {
        var paymentMethodIdResult = PaymentMethodId.Create(paymentMethodId);

        if (paymentMethodIdResult.IsFailure)
            return paymentMethodIdResult.Error;

        var paymentTypeIdResult = PaymentTypeId.Create(paymentTypeId);

        if (paymentTypeIdResult.IsFailure)
            return paymentTypeIdResult.Error;

        var amountResult = Money.Create(amount);

        if (amountResult.IsFailure)
            return amountResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        PaymentMethodId = paymentMethodIdResult.Value;
        PaymentTypeId = paymentTypeIdResult.Value;
        PaymentDate = paymentDate;
        Amount = amountResult.Value;
        Comment = commentResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Payment));
    }

    private static Result<Comment?, Error> CreateComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return (Comment?)null;

        var result = Comment.Create(comment);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}