using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolCategories.CreateToolCategoriesCommand;

public sealed class CreateToolCategoryCommandValidator
    : AbstractValidator<CreateToolCategoryCommand>
{
    public CreateToolCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(ToolCategoryName.Create);
    }
}