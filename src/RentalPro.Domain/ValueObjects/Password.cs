using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed class Password : ValueObject
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 100;

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password, Error> Create(string value)
    {
        value = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(Password));

        if (value.Length < MIN_LENGTH)
        {
            return CommonErrors.LengthIsWrong(
                nameof(Password),
                MIN_LENGTH,
                MAX_LENGTH);
        }

        return new Password(value);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}