using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.CreatePaymentTypeCommand;

public sealed record CreatePaymentTypeCommand(string Name) : IValidation;