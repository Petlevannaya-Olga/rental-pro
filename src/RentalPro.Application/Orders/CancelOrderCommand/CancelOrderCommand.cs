using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CancelOrderCommand;

public sealed record CancelOrderCommand(Guid OrderId) : IValidation;