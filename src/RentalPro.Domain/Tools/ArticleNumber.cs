using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ArticleNumber : ValueObject
{
    public const int MAX_LENGTH = 50;

    public string Value { get; }

    private ArticleNumber(string value)
    {
        Value = value;
    }

    public static Result<ArticleNumber, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim();

        if (value.Length > MAX_LENGTH)
        {
            return CommonErrors.LengthIsWrong(
                nameof(value),
                1,
                MAX_LENGTH);
        }

        return new ArticleNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}