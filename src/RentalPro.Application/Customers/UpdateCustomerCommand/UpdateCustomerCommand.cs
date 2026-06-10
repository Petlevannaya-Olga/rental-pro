using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.UpdateCustomerCommand;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string LastName,
    string FirstName,
    string MiddleName,
    string PhoneNumber,
    string Email,
    string PassportSeries,
    string PassportNumber,
    string PostalCode,
    string Region,
    string City,
    string Street,
    string House,
    string? Building,
    string? Apartment)
    : IValidation;