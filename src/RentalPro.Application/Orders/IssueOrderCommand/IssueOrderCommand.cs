using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.IssueOrderCommand;

public sealed record IssueOrderCommand(
    Guid OrderId)
    : IValidation;