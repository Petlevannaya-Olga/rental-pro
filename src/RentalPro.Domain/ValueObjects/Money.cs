using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Value { get; }

    private Money(decimal value)
    {
        Value = value;
    }

    public static Result<Money, Error> Create(decimal value)
    {
        if (value < 0)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Money cannot be negative");
        }

        return new Money(decimal.Round(value, 2));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}