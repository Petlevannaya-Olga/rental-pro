namespace RentalPro.Contracts.Payments;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerFullName,
    Guid PaymentTypeId,
    string PaymentTypeName,
    Guid PaymentMethodId,
    string PaymentMethodName,
    decimal Amount,
    DateTime OrderDate,
    DateTime PaymentDate,
    string? Comment);