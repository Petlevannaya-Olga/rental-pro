namespace RentalPro.Contracts.Dashboard;

public sealed record DashboardReturnDto(
    Guid OrderId,
    string CustomerFullName,
    string ToolsNames,
    DateOnly PlannedReturnDate,
    int Days);