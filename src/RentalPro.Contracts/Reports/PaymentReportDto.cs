namespace RentalPro.Contracts.Reports;

public sealed record PaymentReportDto(
    string PaymentType,
    int PaymentsCount,
    decimal Amount);