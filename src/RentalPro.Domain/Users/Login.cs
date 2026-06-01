using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Users;

public sealed partial class Login : ValueObject
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 50;

    private static readonly Regex LoginRegex = CreateLoginRegex();

    public string Value { get; }

    private Login(string value)
    {
        Value = value;
    }

    public static Result<Login, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length is < MIN_LENGTH or > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), MIN_LENGTH, MAX_LENGTH);

        if (!LoginRegex.IsMatch(value))
        {
            return CommonErrors.Validation(
                nameof(value),
                "Login can contain only latin letters, digits, dots, underscores and hyphens");
        }

        return new Login(value);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled)]
    private static partial Regex CreateLoginRegex();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}