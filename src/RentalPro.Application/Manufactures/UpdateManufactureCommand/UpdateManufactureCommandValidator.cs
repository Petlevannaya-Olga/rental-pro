using FluentValidation;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Manufactures.UpdateManufactureCommand;

public sealed class UpdateManufacturerCommandValidator
    : AbstractValidator<UpdateManufacturerCommand>
{
    public UpdateManufacturerCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ManufacturerId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(ManufacturerName.Create);
        
        RuleFor(x => x.Name)
            .MustBeValueObject(ManufacturerCountryName.Create);
    }
}