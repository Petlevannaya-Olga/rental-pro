namespace RentalPro.Contracts.Reports;

public sealed record PopularToolReportDto(
    Guid ToolId,
    string ToolName,
    int RentalsCount,
    decimal TotalAmount);