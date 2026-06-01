using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed class Description : ValueObject
{
    public const int MAX_LENGTH = 1000;

    public string Value { get; }

    private Description(string value)
    {
        Value = value;
    }

    public static Result<Description, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Description(string.Empty);

        value = value.Trim();

        if (value.Length > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), 0, MAX_LENGTH);

        return new Description(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}