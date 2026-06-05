namespace RentalPro.Presentation.Client.Models.Dictionaries;

public sealed record DictionaryItem(
    Guid Id,
    string Name,
    string CreatedAt,
    string? UpdatedAt);