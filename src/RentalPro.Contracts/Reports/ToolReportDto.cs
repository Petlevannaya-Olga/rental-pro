namespace RentalPro.Contracts.Reports;

public sealed record ToolReportDto(
    Guid ToolId,
    string ArticleNumber,
    string ToolName,
    string CategoryName,
    string ManufacturerName,
    string StatusName,
    decimal RentalPricePerDay,
    decimal DepositAmount);