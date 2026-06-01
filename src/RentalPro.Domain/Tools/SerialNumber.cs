using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class SerialNumber : ValueObject
{
    public const int MAX_LENGTH = 100;

    public string Value { get; }

    private SerialNumber(string value)
    {
        Value = value;
    }

    public static Result<SerialNumber, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), 1, MAX_LENGTH);

        return new SerialNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}