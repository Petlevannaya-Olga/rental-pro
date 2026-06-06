using FluentValidation;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Manufactures.CreateManufactureCommand;

public sealed class CreateManufacturerCommandValidator
    : AbstractValidator<CreateManufacturerCommand>
{
    public CreateManufacturerCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(ManufacturerName.Create);
        
        RuleFor(x => x.Country)
            .MustBeValueObject(ManufacturerCountryName.Create);
    }
}