using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class OrderItem : AuditableEntity<OrderItemId>
{
    private OrderItem()
        : base(OrderItemId.NewId())
    {
        RentalPricePerDay = null!;
        RentalPeriod = null!;
        ItemTotalCost = null!;
    }

    private OrderItem(
        OrderId orderId,
        ToolId toolId,
        Money rentalPricePerDay,
        RentalPeriod rentalPeriod,
        Money itemTotalCost)
        : base(OrderItemId.NewId())
    {
        OrderId = orderId;
        ToolId = toolId;
        RentalPricePerDay = rentalPricePerDay;
        RentalPeriod = rentalPeriod;
        ItemTotalCost = itemTotalCost;
    }

    public OrderId OrderId { get; private set; }

    public ToolId ToolId { get; private set; }

    public Money RentalPricePerDay { get; private set; }

    public RentalPeriod RentalPeriod { get; private set; }

    public DateOnly? ActualReturnedDate { get; private set; }

    public Money ItemTotalCost { get; private set; }

    public ReturnCondition? ReturnCondition { get; private set; }

    public Comment? DamageComment { get; private set; }

    public static Result<OrderItem, Error> Create(
        Guid orderId,
        Guid toolId,
        decimal rentalPricePerDay,
        DateOnly startDate,
        DateOnly plannedReturnDate)
    {
        var orderIdResult = OrderId.Create(orderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error;

        var toolIdResult = ToolId.Create(toolId);

        if (toolIdResult.IsFailure)
            return toolIdResult.Error;

        var rentalPriceResult = Money.Create(rentalPricePerDay);

        if (rentalPriceResult.IsFailure)
            return rentalPriceResult.Error;

        var rentalPeriodResult = RentalPeriod.Create(
            startDate,
            plannedReturnDate);

        if (rentalPeriodResult.IsFailure)
            return rentalPeriodResult.Error;

        var itemTotalCostResult = CalculateItemTotalCost(
            rentalPriceResult.Value,
            rentalPeriodResult.Value);

        if (itemTotalCostResult.IsFailure)
            return itemTotalCostResult.Error;

        return new OrderItem(
            orderIdResult.Value,
            toolIdResult.Value,
            rentalPriceResult.Value,
            rentalPeriodResult.Value,
            itemTotalCostResult.Value);
    }

    public UnitResult<Error> UpdateRentalPeriod(
        DateOnly startDate,
        DateOnly plannedReturnDate)
    {
        if (ActualReturnedDate.HasValue)
        {
            return CommonErrors.Validation(
                nameof(ActualReturnedDate),
                "Returned order item cannot be changed");
        }

        var rentalPeriodResult = RentalPeriod.Create(
            startDate,
            plannedReturnDate);

        if (rentalPeriodResult.IsFailure)
            return rentalPeriodResult.Error;

        var itemTotalCostResult = CalculateItemTotalCost(
            RentalPricePerDay,
            rentalPeriodResult.Value);

        if (itemTotalCostResult.IsFailure)
            return itemTotalCostResult.Error;

        RentalPeriod = rentalPeriodResult.Value;
        ItemTotalCost = itemTotalCostResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Return(
        DateOnly actualReturnedDate,
        string? returnCondition,
        string? damageComment)
    {
        if (ActualReturnedDate.HasValue)
        {
            return CommonErrors.Validation(
                nameof(ActualReturnedDate),
                "Tool is already returned");
        }

        if (actualReturnedDate < RentalPeriod.StartDate)
        {
            return CommonErrors.Validation(
                nameof(actualReturnedDate),
                "Actual return date cannot be earlier than start date");
        }

        var returnConditionResult = CreateReturnCondition(returnCondition);

        if (returnConditionResult.IsFailure)
            return returnConditionResult.Error;

        var damageCommentResult = CreateDamageComment(damageComment);

        if (damageCommentResult.IsFailure)
            return damageCommentResult.Error;

        ActualReturnedDate = actualReturnedDate;
        ReturnCondition = returnConditionResult.Value;
        DamageComment = damageCommentResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(OrderItem));
    }

    private static Result<Money, Error> CalculateItemTotalCost(
        Money rentalPricePerDay,
        RentalPeriod rentalPeriod)
    {
        var total = rentalPricePerDay.Value * rentalPeriod.DaysCount;

        return Money.Create(total);
    }

    private static Result<ReturnCondition?, Error> CreateReturnCondition(
        string? returnCondition)
    {
        if (string.IsNullOrWhiteSpace(returnCondition))
            return (ReturnCondition?)null;

        var returnConditionResult = ReturnCondition.Create(returnCondition);

        if (returnConditionResult.IsFailure)
            return returnConditionResult.Error;

        return returnConditionResult.Value;
    }

    private static Result<Comment?, Error> CreateDamageComment(
        string? damageComment)
    {
        if (string.IsNullOrWhiteSpace(damageComment))
            return (Comment?)null;

        var damageCommentResult = Comment.Create(damageComment);

        if (damageCommentResult.IsFailure)
            return damageCommentResult.Error;

        return damageCommentResult.Value;
    }
}