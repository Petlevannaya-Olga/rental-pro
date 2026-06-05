using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.CreateToolStatusCommand;

public sealed record CreateToolStatusCommand(string Name) : IValidation;