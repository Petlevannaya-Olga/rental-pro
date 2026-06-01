using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public const int MIN_LENGTH = 10;
    public const int MAX_LENGTH = 20;

    private static readonly Regex PhoneNumberRegex =
        CreatePhoneNumberRegex();

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber, Error> Create(string value)
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

        if (!PhoneNumberRegex.IsMatch(value))
        {
            return CommonErrors.Validation(
                nameof(value),
                "Phone number has invalid format");
        }

        return new PhoneNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^\+?[1-9]\d{9,14}$", RegexOptions.Compiled)]
    private static partial Regex CreatePhoneNumberRegex();
}