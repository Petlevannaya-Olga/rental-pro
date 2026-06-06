using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.CreateRoleCommand;

public sealed record CreateRoleCommand(
    string Name) : IValidation;