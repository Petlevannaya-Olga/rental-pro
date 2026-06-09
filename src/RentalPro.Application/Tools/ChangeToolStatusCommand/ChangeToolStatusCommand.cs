using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.ChangeToolStatusCommand;

public sealed record ChangeToolStatusCommand(
    Guid ToolId,
    Guid StatusId) : IValidation;