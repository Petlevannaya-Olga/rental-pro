using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public readonly record struct PaymentId
{
    public Guid Value { get; }

    private PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId NewId()
    {
        return new PaymentId(Guid.NewGuid());
    }

    public static Result<PaymentId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Payment id cannot be empty");
        }

        return new PaymentId(value);
    }
    
    public static PaymentId Restore(Guid value)
    {
        return new PaymentId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}