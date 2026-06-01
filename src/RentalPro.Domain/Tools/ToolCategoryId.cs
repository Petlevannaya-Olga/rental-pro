using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public readonly record struct ToolCategoryId
{
    public Guid Value { get; }

    private ToolCategoryId(Guid value)
    {
        Value = value;
    }

    public static ToolCategoryId NewId()
    {
        return new ToolCategoryId(Guid.NewGuid());
    }

    public static Result<ToolCategoryId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Tool category id cannot be empty");
        }

        return new ToolCategoryId(value);
    }
    
    public static ToolCategoryId Restore(Guid value)
    {
        return new ToolCategoryId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}