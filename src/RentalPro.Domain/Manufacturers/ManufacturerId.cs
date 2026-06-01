using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Manufacturers;

public readonly record struct ManufacturerId
{
    public Guid Value { get; }

    private ManufacturerId(Guid value)
    {
        Value = value;
    }

    public static ManufacturerId NewId()
    {
        return new ManufacturerId(Guid.NewGuid());
    }

    public static Result<ManufacturerId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Manufacturer id cannot be empty");
        }

        return new ManufacturerId(value);
    }
    
    public static ManufacturerId Restore(Guid value)
    {
        return new ManufacturerId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}