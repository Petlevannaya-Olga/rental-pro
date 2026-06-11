namespace RentalPro.Contracts.Orders;

public sealed record OrderStatsDto(
    int TotalCount,
    int ActiveCount,
    int CompletedCount,
    int OverdueCount);