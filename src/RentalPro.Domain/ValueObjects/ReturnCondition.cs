using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed class ReturnCondition : ValueObject
{
    public const int MAX_LENGTH = 500;

    public string Value { get; }

    private ReturnCondition(string value)
    {
        Value = value;
    }

    public static Result<ReturnCondition, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new ReturnCondition(string.Empty);

        value = value.Trim();

        if (value.Length > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), 0, MAX_LENGTH);

        return new ReturnCondition(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}