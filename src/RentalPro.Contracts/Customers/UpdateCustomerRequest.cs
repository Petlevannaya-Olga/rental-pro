namespace RentalPro.Contracts.Customers;

public sealed record UpdateCustomerRequest(
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
    string? Apartment);