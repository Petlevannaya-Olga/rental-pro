namespace RentalPro.Contracts.Reports;

public sealed record ReportPeriodRequest(
    DateTime DateFrom,
    DateTime DateTo);