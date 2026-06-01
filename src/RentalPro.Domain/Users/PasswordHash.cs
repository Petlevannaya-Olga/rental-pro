using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Users;

public sealed class PasswordHash : ValueObject
{
    public const int MIN_LENGTH = 20;
    public const int MAX_LENGTH = 500;

    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static Result<PasswordHash, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length is < MIN_LENGTH or > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), MIN_LENGTH, MAX_LENGTH);

        return new PasswordHash(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}