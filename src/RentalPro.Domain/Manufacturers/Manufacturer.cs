using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Manufacturers;

public sealed class Manufacturer : AuditableEntity<ManufacturerId>
{
    private Manufacturer(
        ManufacturerName name,
        ManufacturerCountryName country)
        : base(ManufacturerId.NewId())
    {
        Name = name;
        Country = country;
    }

    public ManufacturerName Name { get; private set; }

    public ManufacturerCountryName Country { get; private set; }

    public static Result<Manufacturer, Error> Create(
        string name,
        string country)
    {
        var nameResult = ManufacturerName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var countryResult = ManufacturerCountryName.Create(country);

        if (countryResult.IsFailure)
            return countryResult.Error;

        return new Manufacturer(
            nameResult.Value,
            countryResult.Value);
    }

    public UnitResult<Error> Update(
        string name,
        string? description)
    {
        var nameResult = ManufacturerName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var countryResult = ManufacturerCountryName.Create(name);

        if (countryResult.IsFailure)
            return countryResult.Error;

        Name = nameResult.Value;
        Country = countryResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Manufacturer));
    }
}