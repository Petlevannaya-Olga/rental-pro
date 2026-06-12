namespace RentalPro.Contracts.Payments;

public sealed record FiscalReceiptResult(
    string ReceiptId,
    string Status,
    DateTime FiscalizedAt,
    string? ErrorMessage);