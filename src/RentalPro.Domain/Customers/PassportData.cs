using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Customers;

public sealed partial class PassportData : ValueObject
{
    private static readonly Regex SeriesRegex = CreateSeriesRegex();

    private static readonly Regex NumberRegex = CreateNumberRegex();
    
    public const int SERIES_LENGTH = 4;

    public const int NUMBER_LENGTH = 6;

    public string Series { get; }

    public string Number { get; }

    private PassportData(
        string series,
        string number)
    {
        Series = series;
        Number = number;
    }

    public static Result<PassportData, Error> Create(
        string series,
        string number)
    {
        if (string.IsNullOrWhiteSpace(series))
            return CommonErrors.IsRequired(nameof(series));

        if (string.IsNullOrWhiteSpace(number))
            return CommonErrors.IsRequired(nameof(number));

        series = series.Trim();
        number = number.Trim();

        if (!SeriesRegex.IsMatch(series))
        {
            return CommonErrors.Validation(
                nameof(series),
                "Passport series must contain 4 digits");
        }

        if (!NumberRegex.IsMatch(number))
        {
            return CommonErrors.Validation(
                nameof(number),
                "Passport number must contain 6 digits");
        }

        return new PassportData(series, number);
    }

   protected override IEnumerable<object> GetEqualityComponents()
   {
       yield return Series;
       yield return Number;
   }

    [GeneratedRegex(@"^\d{4}$", RegexOptions.Compiled)]
    private static partial Regex CreateSeriesRegex();

    [GeneratedRegex(@"^\d{6}$", RegexOptions.Compiled)]
    private static partial Regex CreateNumberRegex();
}