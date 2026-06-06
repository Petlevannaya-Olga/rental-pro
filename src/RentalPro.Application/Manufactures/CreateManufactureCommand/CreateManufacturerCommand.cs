using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.CreateManufactureCommand;

public  sealed record CreateManufacturerCommand(string Name, string Country) : IValidation;