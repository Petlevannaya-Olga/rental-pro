namespace RentalPro.Contracts.ToolCategories;

public sealed record ToolCategoryDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);