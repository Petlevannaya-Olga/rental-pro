using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.DeleteToolCommand;

public sealed record DeleteToolCommand(Guid Id) : IValidation;