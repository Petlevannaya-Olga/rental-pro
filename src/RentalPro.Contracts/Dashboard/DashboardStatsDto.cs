namespace RentalPro.Contracts.Dashboard;

public sealed record DashboardStatsDto(
    int TotalTools,
    int RentedTools,
    int BookedTools,
    int ToolsInRepair,
    int TotalCustomers,
    decimal MonthlyRevenue);