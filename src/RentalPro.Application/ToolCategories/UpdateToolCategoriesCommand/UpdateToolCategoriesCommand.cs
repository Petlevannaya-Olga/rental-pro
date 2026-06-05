using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.UpdateToolCategoriesCommand;

public sealed record UpdateToolCategoryCommand(
    Guid Id,
    string Name) : IValidation;