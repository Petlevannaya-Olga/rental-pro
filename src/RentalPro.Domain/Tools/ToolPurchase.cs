using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Suppliers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolPurchase : AuditableEntity<ToolPurchaseId>
{
    private ToolPurchase(
        SupplierId supplierId,
        ToolId toolId,
        DateOnly purchaseDate,
        Money price,
        Comment? comment)
        : base(ToolPurchaseId.NewId())
    {
        SupplierId = supplierId;
        ToolId = toolId;
        PurchaseDate = purchaseDate;
        Price = price;
        Comment = comment;
    }

    public SupplierId SupplierId { get; private set; }

    public ToolId ToolId { get; private set; }

    public DateOnly PurchaseDate { get; private set; }

    public Money Price { get; private set; }

    public Comment? Comment { get; private set; }

    public static Result<ToolPurchase, Error> Create(
        Guid supplierId,
        Guid toolId,
        DateOnly purchaseDate,
        decimal price,
        string? comment)
    {
        var supplierIdResult = SupplierId.Create(supplierId);

        if (supplierIdResult.IsFailure)
            return supplierIdResult.Error;

        var toolIdResult = ToolId.Create(toolId);

        if (toolIdResult.IsFailure)
            return toolIdResult.Error;

        var priceResult = Money.Create(price);

        if (priceResult.IsFailure)
            return priceResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new ToolPurchase(
            supplierIdResult.Value,
            toolIdResult.Value,
            purchaseDate,
            priceResult.Value,
            commentResult.Value);
    }

    public UnitResult<Error> Update(
        DateOnly purchaseDate,
        decimal price,
        string? comment)
    {
        var priceResult = Money.Create(price);

        if (priceResult.IsFailure)
            return priceResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        PurchaseDate = purchaseDate;
        Price = priceResult.Value;
        Comment = commentResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeSupplier(Guid supplierId)
    {
        var supplierIdResult = SupplierId.Create(supplierId);

        if (supplierIdResult.IsFailure)
            return supplierIdResult.Error;

        SupplierId = supplierIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeTool(Guid toolId)
    {
        var toolIdResult = ToolId.Create(toolId);

        if (toolIdResult.IsFailure)
            return toolIdResult.Error;

        ToolId = toolIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(ToolPurchase));
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