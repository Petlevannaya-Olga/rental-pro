using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.UpdateRoleCommand;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name) : ICommand;