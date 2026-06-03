using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Users.ChangeUserPasswordCommand;

public sealed record ChangeUserPasswordCommand(
    Guid UserId,
    string OldPassword,
    string NewPassword) : ICommand;