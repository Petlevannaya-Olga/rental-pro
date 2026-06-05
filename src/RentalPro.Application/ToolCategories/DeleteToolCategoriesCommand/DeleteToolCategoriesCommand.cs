using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolCategories.DeleteToolCategoriesCommand;

public sealed record DeleteToolCategoryCommand(Guid Id) : IValidation;