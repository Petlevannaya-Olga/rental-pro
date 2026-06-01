using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public readonly record struct PaymentTypeId
{
    public Guid Value { get; }

    private PaymentTypeId(Guid value)
    {
        Value = value;
    }

    public static PaymentTypeId NewId()
    {
        return new PaymentTypeId(Guid.NewGuid());
    }

    public static Result<PaymentTypeId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Payment type id cannot be empty");
        }

        return new PaymentTypeId(value);
    }
    
    public static PaymentTypeId Restore(Guid value)
    {
        return new PaymentTypeId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}