namespace RentalPro.Contracts.Customers;

public sealed record CustomerDto(
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
    string? Apartment,
    int OrdersCount,
    int ActiveOrdersCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public string FullName =>
        $"{LastName} {FirstName} {MiddleName}";

    public string Passport =>
        $"{PassportSeries} {PassportNumber}";

    public string Address =>
        string.Join(", ",
            new[]
            {
                PostalCode,
                Region,
                City,
                Street,
                string.IsNullOrWhiteSpace(House) ? null : $"д. {House}",
                string.IsNullOrWhiteSpace(Building) ? null : $"к. {Building}",
                string.IsNullOrWhiteSpace(Apartment) ? null : $"кв. {Apartment}"
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
}