namespace RentalPro.Presentation.Client.Models;

public sealed class ErrorResponse
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; }
    public string? InvalidField { get; set; }
}