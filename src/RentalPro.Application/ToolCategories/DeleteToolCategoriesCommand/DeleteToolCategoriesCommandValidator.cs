using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolCategories.DeleteToolCategoriesCommand;

public sealed class DeleteToolCategoryCommandValidator
    : AbstractValidator<DeleteToolCategoryCommand>
{
    public DeleteToolCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolCategoryId.Create);
    }
}