using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolCategories.UpdateToolCategoriesCommand;

public sealed class UpdateToolCategoryCommandValidator
    : AbstractValidator<UpdateToolCategoryCommand>
{
    public UpdateToolCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolCategoryId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(ToolCategoryName.Create);
    }
}