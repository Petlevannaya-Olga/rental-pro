using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public readonly record struct ToolPurchaseId
{
    public Guid Value { get; }

    private ToolPurchaseId(Guid value)
    {
        Value = value;
    }

    public static ToolPurchaseId NewId()
    {
        return new ToolPurchaseId(Guid.NewGuid());
    }

    public static Result<ToolPurchaseId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Tool purchase id cannot be empty");
        }

        return new ToolPurchaseId(value);
    }

    public static ToolPurchaseId Restore(Guid value)
    {
        return new ToolPurchaseId(value);
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
}