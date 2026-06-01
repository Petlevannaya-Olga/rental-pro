using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolCategoryName : ValueObject
{
    public const int MIN_LENGTH = 2;
    public const int MAX_LENGTH = 100;

    public string Value { get; }

    private ToolCategoryName(string value)
    {
        Value = value;
    }

    public static Result<ToolCategoryName, Error> Create(string value)
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

        return new ToolCategoryName(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}