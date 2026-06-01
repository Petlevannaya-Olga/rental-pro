using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public sealed class PaymentMethodName : ValueObject
{
    public const int MIN_LENGTH = 2;
    public const int MAX_LENGTH = 100;

    public string Value { get; }

    private PaymentMethodName(string value)
    {
        Value = value;
    }

    public static Result<PaymentMethodName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length is < MIN_LENGTH or > MAX_LENGTH)
        {
            return CommonErrors.LengthIsWrong(
                nameof(value),
                MIN_LENGTH,
                MAX_LENGTH);
        }

        return new PaymentMethodName(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}