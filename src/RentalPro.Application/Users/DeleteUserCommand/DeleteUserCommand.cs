using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.DeleteUserCommand;

public sealed record DeleteUserCommand(Guid UserId) : IValidation;