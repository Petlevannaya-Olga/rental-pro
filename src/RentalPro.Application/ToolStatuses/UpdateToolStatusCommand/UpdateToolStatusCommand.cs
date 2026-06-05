using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.UpdateToolStatusCommand;

public sealed record UpdateToolStatusCommand(
    Guid Id,
    string Name) : IValidation;