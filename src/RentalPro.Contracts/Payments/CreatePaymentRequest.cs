namespace RentalPro.Contracts.Payments;

public sealed record CreatePaymentRequest(
    Guid OrderId,
    Guid PaymentTypeId,
    Guid PaymentMethodId,
    decimal Amount,
    DateTime PaymentDate,
    string? Comment);