using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Fines;

public sealed class FineReason : ValueObject
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 1000;

    public string Value { get; }

    private FineReason(string value)
    {
        Value = value;
    }

    public static Result<FineReason, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length is < MIN_LENGTH or > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), MIN_LENGTH, MAX_LENGTH);

        return new FineReason(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}