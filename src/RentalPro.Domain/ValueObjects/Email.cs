using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = CreateEmailRegex();

    public const int MAX_LENGTH = 254;
    
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (!EmailRegex.IsMatch(value))
            return CommonErrors.Validation(
                nameof(value),
                "Email has invalid format");

        return new Email(value);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ru-RU")]
    private static partial Regex CreateEmailRegex();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}