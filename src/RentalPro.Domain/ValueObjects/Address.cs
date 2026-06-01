using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.ValueObjects;

public sealed partial class Address : ValueObject
{
    public const int POSTAL_CODE_LENGTH = 6;

    public const int MAX_REGION_LENGTH = 100;
    public const int MAX_CITY_LENGTH = 100;
    public const int MAX_STREET_LENGTH = 200;
    public const int MAX_HOUSE_LENGTH = 20;
    public const int MAX_BUILDING_LENGTH = 20;
    public const int MAX_APARTMENT_LENGTH = 20;

    private static readonly Regex PostalCodeRegex = CreatePostalCodeRegex();

    public string PostalCode { get; }

    public string Region { get; }

    public string City { get; }

    public string Street { get; }

    public string House { get; }

    public string? Building { get; }

    public string? Apartment { get; }

    private Address(
        string postalCode,
        string region,
        string city,
        string street,
        string house,
        string? building,
        string? apartment)
    {
        PostalCode = postalCode;
        Region = region;
        City = city;
        Street = street;
        House = house;
        Building = building;
        Apartment = apartment;
    }

    public static Result<Address, Error> Create(
        string postalCode,
        string region,
        string city,
        string street,
        string house,
        string? building,
        string? apartment)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return CommonErrors.IsRequired(nameof(postalCode));

        if (string.IsNullOrWhiteSpace(region))
            return CommonErrors.IsRequired(nameof(region));

        if (string.IsNullOrWhiteSpace(city))
            return CommonErrors.IsRequired(nameof(city));

        if (string.IsNullOrWhiteSpace(street))
            return CommonErrors.IsRequired(nameof(street));

        if (string.IsNullOrWhiteSpace(house))
            return CommonErrors.IsRequired(nameof(house));

        postalCode = postalCode.Trim();
        region = region.Trim();
        city = city.Trim();
        street = street.Trim();
        house = house.Trim();
        building = string.IsNullOrWhiteSpace(building) ? null : building.Trim();
        apartment = string.IsNullOrWhiteSpace(apartment) ? null : apartment.Trim();

        if (!PostalCodeRegex.IsMatch(postalCode))
        {
            return CommonErrors.Validation(
                nameof(postalCode),
                "Postal code must contain exactly 6 digits");
        }

        if (region.Length > MAX_REGION_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(region), 1, MAX_REGION_LENGTH);

        if (city.Length > MAX_CITY_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(city), 1, MAX_CITY_LENGTH);

        if (street.Length > MAX_STREET_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(street), 1, MAX_STREET_LENGTH);

        if (house.Length > MAX_HOUSE_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(house), 1, MAX_HOUSE_LENGTH);

        if (building is not null && building.Length > MAX_BUILDING_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(building), 0, MAX_BUILDING_LENGTH);

        if (apartment is not null && apartment.Length > MAX_APARTMENT_LENGTH)
            return CommonErrors.LengthIsWrong(nameof(apartment), 0, MAX_APARTMENT_LENGTH);

        return new Address(
            postalCode,
            region,
            city,
            street,
            house,
            building,
            apartment);
    }

    public override string ToString()
    {
        var buildingPart = Building is null
            ? string.Empty
            : $", корп. {Building}";

        var apartmentPart = Apartment is null
            ? string.Empty
            : $", кв./офис {Apartment}";

        return $"{PostalCode}, {Region}, {City}, {Street}, д. {House}{buildingPart}{apartmentPart}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PostalCode;
        yield return Region;
        yield return City;
        yield return Street;
        yield return House;
        yield return Building ?? string.Empty;
        yield return Apartment ?? string.Empty;
    }

    [GeneratedRegex(@"^\d{6}$", RegexOptions.Compiled)]
    private static partial Regex CreatePostalCodeRegex();
}