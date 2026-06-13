namespace RentalPro.Contracts.Dashboard;

public sealed record DashboardDto(
    DashboardStatsDto Stats,
    List<DashboardRecentOrderDto> RecentOrders,
    DashboardToolStatusesDto ToolStatuses,
    List<DashboardReturnDto> UpcomingReturns,
    List<DashboardReturnDto> OverdueReturns);