using RentalPro.Domain.Users;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.ChangeUserStatusCommand;

public sealed record ChangeUserStatusCommand(
    UserId UserId,
    bool IsActive) : IValidation;