namespace RentalPro.Contracts.Payments;

public sealed record PaymentStatsDto(
    int TotalCount,
    int RentalCount,
    int DepositCount,
    int DepositRefundCount,
    decimal TotalAmount);