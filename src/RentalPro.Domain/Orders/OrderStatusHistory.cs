using CSharpFunctionalExtensions;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class OrderStatusHistory
{
    private OrderStatusHistory(
        OrderId orderId,
        OrderStatusId? oldStatusId,
        OrderStatusId newStatusId,
        UserId userId,
        DateTime changeDate,
        Comment? comment)
    {
        Id = OrderStatusHistoryId.NewId();

        OrderId = orderId;
        OldStatusId = oldStatusId;
        NewStatusId = newStatusId;
        UserId = userId;
        ChangeDate = changeDate;
        Comment = comment;

        CreatedAt = DateTime.UtcNow;
    }

    public OrderStatusHistoryId Id { get; private set; }

    public OrderId OrderId { get; private set; }

    public OrderStatusId? OldStatusId { get; private set; }

    public OrderStatusId NewStatusId { get; private set; }

    public UserId UserId { get; private set; }

    public DateTime ChangeDate { get; private set; }

    public Comment? Comment { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<OrderStatusHistory, Error> Create(
        Guid orderId,
        Guid? oldStatusId,
        Guid newStatusId,
        Guid userId,
        DateTime changeDate,
        string? comment)
    {
        var orderIdResult = OrderId.Create(orderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error;

        var oldStatusIdResult = CreateOldStatusId(oldStatusId);

        if (oldStatusIdResult.IsFailure)
            return oldStatusIdResult.Error;

        var newStatusIdResult = OrderStatusId.Create(newStatusId);

        if (newStatusIdResult.IsFailure)
            return newStatusIdResult.Error;

        var userIdResult = UserId.Create(userId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new OrderStatusHistory(
            orderIdResult.Value,
            oldStatusIdResult.Value,
            newStatusIdResult.Value,
            userIdResult.Value,
            changeDate,
            commentResult.Value);
    }

    private static Result<OrderStatusId?, Error> CreateOldStatusId(Guid? oldStatusId)
    {
        if (oldStatusId is null)
            return (OrderStatusId?)null;

        var result = OrderStatusId.Create(oldStatusId.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
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