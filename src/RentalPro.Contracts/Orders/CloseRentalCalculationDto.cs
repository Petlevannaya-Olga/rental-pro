namespace RentalPro.Contracts.Orders;

public sealed record CloseRentalCalculationDto(
    decimal PlannedRentalAmount,
    decimal ActualRentalAmount,

    decimal PaidRentalAmount,
    decimal PaidDepositAmount,

    decimal RefundedRentalAmount,
    decimal RefundedDepositAmount,

    decimal RentalBalanceAmount,
    decimal RentalRefundAmount,
    decimal RentalAdditionalPaymentAmount,

    decimal DepositRefundAmount);