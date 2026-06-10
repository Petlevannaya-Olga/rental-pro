namespace RentalPro.Presentation.Client.Models.Customers;

public sealed class CustomerFormModel
{
    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PassportSeries { get; set; } = string.Empty;

    public string PassportNumber { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string House { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public string Apartment { get; set; } = string.Empty;

    public string FullName =>
        $"{LastName} {FirstName} {MiddleName}";
}