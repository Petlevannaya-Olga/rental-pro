using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Customers;

public readonly record struct CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId NewId()
    {
        return new CustomerId(Guid.NewGuid());
    }

    public static Result<CustomerId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Customer id cannot be empty");
        }

        return new CustomerId(value);
    }

    public static CustomerId Restore(Guid value)
    {
        return new CustomerId(value);
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
}