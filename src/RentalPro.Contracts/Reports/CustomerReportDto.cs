namespace RentalPro.Contracts.Reports;

public sealed record CustomerReportDto(
    Guid CustomerId,
    string CustomerFullName,
    int OrdersCount,
    decimal RentAmount,
    DateTime? LastOrderDate);