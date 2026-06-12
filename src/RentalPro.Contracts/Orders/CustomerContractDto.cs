namespace RentalPro.Contracts.Orders;

public sealed record CustomerContractDto(
    string FullName,
    string Passport,
    string Address,
    string Phone,
    string Email);