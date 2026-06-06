using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.DeleteRoleCommand;

public sealed record DeleteRoleCommand(
    Guid Id) : IValidation;