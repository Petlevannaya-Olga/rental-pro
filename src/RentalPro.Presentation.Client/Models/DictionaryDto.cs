namespace RentalPro.Presentation.Client.Models;

public sealed record DictionaryDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);