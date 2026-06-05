namespace RentalPro.Contracts.PaymentTypes;

public sealed record PaymentTypeDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);