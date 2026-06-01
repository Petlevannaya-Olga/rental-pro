using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public readonly record struct ToolStatusId
{
    public Guid Value { get; }

    private ToolStatusId(Guid value)
    {
        Value = value;
    }

    public static ToolStatusId NewId()
    {
        return new ToolStatusId(Guid.NewGuid());
    }

    public static Result<ToolStatusId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Tool status id cannot be empty");
        }

        return new ToolStatusId(value);
    }
    
    public static ToolStatusId Restore(Guid value)
    {
        return new ToolStatusId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}