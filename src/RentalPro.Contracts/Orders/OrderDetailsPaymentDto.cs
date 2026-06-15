namespace RentalPro.Contracts.Orders;

public sealed record OrderDetailsPaymentDto(
    Guid Id,
    string PaymentTypeName,
    string PaymentMethodName,
    decimal Amount,
    DateTime PaymentDate,
    string? Comment);