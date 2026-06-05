using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.CreateToolCategoriesCommand;

public sealed record CreateToolCategoryCommand(string Name) : IValidation;