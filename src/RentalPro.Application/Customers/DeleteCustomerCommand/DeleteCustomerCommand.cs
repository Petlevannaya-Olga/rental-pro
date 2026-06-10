using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.DeleteCustomerCommand;

public sealed record DeleteCustomerCommand(Guid Id) : IValidation;