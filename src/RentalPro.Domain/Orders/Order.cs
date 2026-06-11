using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class Order : AuditableEntity<OrderId>
{
    private readonly List<OrderItem> _items = [];

    // EF Core
    private Order()
        : base(OrderId.NewId())
    {
    }

    private Order(
        UserId userId,
        CustomerId customerId,
        DateTime orderDate,
        OrderStatusId statusId,
        Comment? comment)
        : base(OrderId.NewId())
    {
        Number = OrderNumber.Generate(Id, orderDate);
        UserId = userId;
        CustomerId = customerId;
        OrderDate = orderDate;
        StatusId = statusId;
        Comment = comment;
    }

    public UserId UserId { get; private set; }

    public User User { get; private set; } = null!;

    public CustomerId CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public DateTime OrderDate { get; private set; }

    public OrderNumber Number { get; private set; }

    public OrderStatusId StatusId { get; private set; }

    public OrderStatus Status { get; private set; } = null!;

    public Comment? Comment { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public static Result<Order, Error> Create(
        Guid userId,
        Guid customerId,
        DateTime orderDate,
        Guid statusId,
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

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new Order(
            userIdResult.Value,
            customerIdResult.Value,
            orderDate,
            statusIdResult.Value,
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