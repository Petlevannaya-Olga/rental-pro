using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Suppliers;

public readonly record struct SupplierId
{
    public Guid Value { get; }

    private SupplierId(Guid value)
    {
        Value = value;
    }

    public static SupplierId NewId()
    {
        return new SupplierId(Guid.NewGuid());
    }

    public static Result<SupplierId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Supplier id cannot be empty");
        }

        return new SupplierId(value);
    }
    
    public static SupplierId Restore(Guid value)
    {
        return new SupplierId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}