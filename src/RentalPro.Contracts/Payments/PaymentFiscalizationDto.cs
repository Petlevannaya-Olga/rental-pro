namespace RentalPro.Contracts.Payments;

public sealed record PaymentFiscalizationDto(
    Guid PaymentId,
    string PaymentNumber,
    Guid OrderId,
    string OrderNumber,
    string CustomerFullName,
    string CustomerEmail,
    string CustomerPhone,
    string PaymentTypeName,
    string PaymentMethodName,
    decimal Amount,
    DateTime PaymentDate);