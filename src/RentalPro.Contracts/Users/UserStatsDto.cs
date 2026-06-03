namespace RentalPro.Contracts.Users;

public sealed record UserStatsDto(
    int TotalCount,
    int ActiveCount,
    int AdminCount,
    int BlockedCount);