using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.DeletePaymentTypeCommand;

public sealed record DeletePaymentTypeCommand(Guid Id) : IValidation;
