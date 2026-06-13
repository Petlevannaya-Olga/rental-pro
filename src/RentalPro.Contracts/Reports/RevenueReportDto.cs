namespace RentalPro.Contracts.Reports;

public sealed record RevenueReportDto(
    DateTime Date,
    int PaymentsCount,
    decimal Amount);