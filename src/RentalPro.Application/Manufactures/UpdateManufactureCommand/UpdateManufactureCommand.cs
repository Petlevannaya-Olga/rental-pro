using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.UpdateManufactureCommand;

public sealed record UpdateManufacturerCommand(
    Guid Id,
    string Name,
    string Country) : IValidation;