namespace RentalPro.Presentation.Client.Models.Dictionaries;

public sealed record DictionaryItem(
    Guid Id,
    string Name,
    string? Country,
    string CreatedAt,
    string? UpdatedAt);