using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Fines;

public readonly record struct FineId
{
    public Guid Value { get; }

    private FineId(Guid value)
    {
        Value = value;
    }

    public static FineId NewId()
    {
        return new FineId(Guid.NewGuid());
    }

    public static Result<FineId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Fine id cannot be empty");
        }

        return new FineId(value);
    }

    public static FineId Restore(Guid value)
    {
        return new FineId(value);
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
}