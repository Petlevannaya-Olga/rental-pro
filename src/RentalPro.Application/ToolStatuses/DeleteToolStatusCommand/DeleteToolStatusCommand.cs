using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.DeleteToolStatusCommand;

public sealed record DeleteToolStatusCommand(Guid Id) : IValidation;