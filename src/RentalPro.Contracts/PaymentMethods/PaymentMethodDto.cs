namespace RentalPro.Contracts.PaymentMethods;

public sealed record PaymentMethodDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);