namespace RentalPro.Presentation.Client.Models;

public sealed record DictionaryDto(
    Guid Id,
    string Name,
    string? Country,
    DateTime CreatedAt,
    DateTime? UpdatedAt);