using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class Order : AuditableEntity<OrderId>
{
    private Order(
        UserId userId,
        CustomerId customerId,
        DateTime orderDate,
        OrderStatusId statusId,
        Money totalCost,
        Money depositTotal,
        Comment? comment)
        : base(OrderId.NewId())
    {
        UserId = userId;
        CustomerId = customerId;
        OrderDate = orderDate;
        StatusId = statusId;
        TotalCost = totalCost;
        DepositTotal = depositTotal;
        Comment = comment;
    }

    public UserId UserId { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public DateTime OrderDate { get; private set; }

    public OrderStatusId StatusId { get; private set; }

    public Money TotalCost { get; private set; }

    public Money DepositTotal { get; private set; }

    public Comment? Comment { get; private set; }

    public static Result<Order, Error> Create(
        Guid userId,
        Guid customerId,
        DateTime orderDate,
        Guid statusId,
        decimal totalCost,
        decimal depositTotal,
        string? comment)
    {
        var userIdResult = UserId.Create(userId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var customerIdResult = CustomerId.Create(customerId);

        if (customerIdResult.IsFailure)
            return customerIdResult.Error;

        var statusIdResult = OrderStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        var totalCostResult = Money.Create(totalCost);

        if (totalCostResult.IsFailure)
            return totalCostResult.Error;

        var depositTotalResult = Money.Create(depositTotal);

        if (depositTotalResult.IsFailure)
            return depositTotalResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new Order(
            userIdResult.Value,
            customerIdResult.Value,
            orderDate,
            statusIdResult.Value,
            totalCostResult.Value,
            depositTotalResult.Value,
            commentResult.Value);
    }

    public UnitResult<Error> ChangeStatus(Guid statusId)
    {
        var statusIdResult = OrderStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        StatusId = statusIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateTotals(
        decimal totalCost,
        decimal depositTotal)
    {
        var totalCostResult = Money.Create(totalCost);

        if (totalCostResult.IsFailure)
            return totalCostResult.Error;

        var depositTotalResult = Money.Create(depositTotal);

        if (depositTotalResult.IsFailure)
            return depositTotalResult.Error;

        TotalCost = totalCostResult.Value;
        DepositTotal = depositTotalResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateComment(string? comment)
    {
        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        Comment = commentResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Order));
    }

    private static Result<Comment?, Error> CreateComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return (Comment?)null;

        var commentResult = Comment.Create(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return commentResult.Value;
    }
}