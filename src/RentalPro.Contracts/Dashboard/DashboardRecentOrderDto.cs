namespace RentalPro.Contracts.Dashboard;

public sealed record DashboardRecentOrderDto(
    Guid Id,
    string Number,
    string CustomerFullName,
    int ToolsCount,
    DateOnly? StartDate,
    DateOnly? PlannedReturnDate,
    string StatusName,
    decimal TotalCost);