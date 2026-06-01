using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed class FullName : ValueObject
{
    public const int MIN_LENGTH = 2;

    public const int MAX_LENGTH = 100;

    public string LastName { get; }

    public string FirstName { get; }

    public string MiddleName { get; }

    private FullName(
        string lastName,
        string firstName,
        string middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
    }

    public static Result<FullName, Error> Create(
        string lastName,
        string firstName,
        string middleName)
    {
        lastName = lastName?.Trim() ?? string.Empty;
        firstName = firstName?.Trim() ?? string.Empty;
        middleName = middleName?.Trim() ?? string.Empty;

        var validationResult = ValidatePart(
            nameof(lastName),
            lastName);

        if (validationResult.IsFailure)
            return validationResult.Error;

        validationResult = ValidatePart(
            nameof(firstName),
            firstName);

        if (validationResult.IsFailure)
            return validationResult.Error;

        validationResult = ValidatePart(
            nameof(middleName),
            middleName);

        if (validationResult.IsFailure)
            return validationResult.Error;

        return new FullName(
            lastName,
            firstName,
            middleName);
    }

    public static UnitResult<Error> ValidatePart(
        string fieldName,
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(fieldName);

        if (value.Length is < MIN_LENGTH or > MAX_LENGTH)
        {
            return CommonErrors.LengthIsWrong(
                fieldName,
                MIN_LENGTH,
                MAX_LENGTH);
        }

        return UnitResult.Success<Error>();
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return LastName;
        yield return FirstName;
        yield return MiddleName;
    }

    public override string ToString()
    {
        return $"{LastName} {FirstName} {MiddleName}";
    }
}