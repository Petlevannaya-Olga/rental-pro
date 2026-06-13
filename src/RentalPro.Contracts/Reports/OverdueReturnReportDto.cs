namespace RentalPro.Contracts.Reports;

public sealed record OverdueReturnReportDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerFullName,
    string ToolsNames,
    DateTime PlannedReturnDate,
    int OverdueDays);