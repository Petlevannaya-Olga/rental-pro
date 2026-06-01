using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public readonly record struct ToolId
{
    public Guid Value { get; }

    private ToolId(Guid value)
    {
        Value = value;
    }

    public static ToolId NewId()
    {
        return new ToolId(Guid.NewGuid());
    }

    public static Result<ToolId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Tool id cannot be empty");
        }

        return new ToolId(value);
    }
    
    public static ToolId Restore(Guid value)
    {
        return new ToolId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}