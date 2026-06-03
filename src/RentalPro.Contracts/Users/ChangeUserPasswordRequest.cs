namespace RentalPro.Contracts.Users;

public sealed record ChangeUserPasswordRequest
{
    public string OldPassword { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}