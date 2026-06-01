using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public readonly record struct ToolStatusHistoryId
{
    public Guid Value { get; }

    private ToolStatusHistoryId(Guid value)
    {
        Value = value;
    }

    public static ToolStatusHistoryId NewId()
    {
        return new ToolStatusHistoryId(Guid.NewGuid());
    }

    public static Result<ToolStatusHistoryId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Tool status history id cannot be empty");
        }

        return new ToolStatusHistoryId(value);
    }
    
    public static ToolStatusHistoryId Restore(Guid value)
    {
        return new ToolStatusHistoryId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}