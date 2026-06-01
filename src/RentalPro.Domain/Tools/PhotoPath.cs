using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class PhotoPath : ValueObject
{
    public const int MAX_LENGTH = 500;

    public string Value { get; }

    private PhotoPath(string value)
    {
        Value = value;
    }

    public static Result<PhotoPath, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new PhotoPath(string.Empty);

        value = value.Trim();

        if (value.Length > MAX_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(value), 0, MAX_LENGTH);

        return new PhotoPath(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}