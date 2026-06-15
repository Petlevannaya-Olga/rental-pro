namespace RentalPro.Contracts.Orders;

public sealed record CloseRentalRequest(
    Guid PaymentMethodId,
    DateTime PaymentDate,
    string? Comment);