using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Manufacturers;

public sealed class ManufacturerName : ValueObject
{
    public const int MIN_LENGTH = 2;
    public const int MAX_LENGTH = 200;

    public string Value { get; }

    private ManufacturerName(string value)
    {
        Value = value;
    }

    public static Result<ManufacturerName, Error> Create(string value)
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

        return new ManufacturerName(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}