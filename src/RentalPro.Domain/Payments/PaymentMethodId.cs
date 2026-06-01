using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public readonly record struct PaymentMethodId
{
    public Guid Value { get; }

    private PaymentMethodId(Guid value)
    {
        Value = value;
    }

    public static PaymentMethodId NewId()
    {
        return new PaymentMethodId(Guid.NewGuid());
    }

    public static Result<PaymentMethodId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Payment method id cannot be empty");
        }

        return new PaymentMethodId(value);
    }

    public static PaymentMethodId Restore(Guid value)
    {
        return new PaymentMethodId(value);
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }
}