namespace RentalPro.Contracts.Tools;

public sealed record ToolStatsDto(
    int TotalCount,
    int AvailableCount,
    int RentedCount,
    int RepairCount);