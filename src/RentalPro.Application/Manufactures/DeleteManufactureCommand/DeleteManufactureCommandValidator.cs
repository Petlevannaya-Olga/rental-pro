using FluentValidation;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Manufactures.DeleteManufactureCommand;

public sealed class DeleteManufacturerCommandValidator
    : AbstractValidator<DeleteManufacturerCommand>
{
    public DeleteManufacturerCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ManufacturerId.Create);
    }
}