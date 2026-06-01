using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Roles;

public sealed class RoleName : ValueObject
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 100;

    public string Value { get; }

    private RoleName(string value)
    {
        Value = value;
    }

    public static Result<RoleName, Error> Create(string value)
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

        return new RoleName(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}