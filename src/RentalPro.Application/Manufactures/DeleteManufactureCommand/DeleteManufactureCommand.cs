using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.DeleteManufactureCommand;

public sealed record DeleteManufacturerCommand(Guid Id) : IValidation;