namespace RentalPro.Contracts.Customers;

public sealed record CreateCustomerResponse(
    Guid Id,
    string FullName);