namespace RentalPro.Presentation.Client.Models.Users;

public sealed record EditUserFormModel
{
    public Guid Id { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public Guid? RoleId { get; set; }
}