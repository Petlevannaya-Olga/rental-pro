namespace RentalPro.Contracts.Reports;

public sealed record ToolRepairReportDto(
    Guid ToolId,
    string ToolName,
    DateTime RepairDate,
    string Description,
    decimal Cost);