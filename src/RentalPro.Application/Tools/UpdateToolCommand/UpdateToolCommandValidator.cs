using FluentValidation;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Tools.UpdateToolCommand;

public sealed class UpdateToolCommandValidator
    : AbstractValidator<UpdateToolCommand>
{
    public UpdateToolCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolId.Create);

        RuleFor(x => x.ArticleNumber)
            .MustBeValueObject(ArticleNumber.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(ToolName.Create);

        RuleFor(x => x.Description)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           Description.Create(value).IsSuccess)
            .WithMessage("Invalid description");

        RuleFor(x => x.CategoryId)
            .MustBeValueObject(ToolCategoryId.Create);

        RuleFor(x => x.ManufacturerId)
            .MustBeValueObject(ManufacturerId.Create);

        RuleFor(x => x.RentalPricePerDay)
            .MustBeValueObject(Money.Create);

        RuleFor(x => x.DepositAmount)
            .MustBeValueObject(Money.Create);

        RuleFor(x => x.SerialNumber)
            .MustBeValueObject(SerialNumber.Create);

        RuleFor(x => x.InventoryNumber)
            .MustBeValueObject(InventoryNumber.Create);

        RuleFor(x => x.CurrentCondition)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           ReturnCondition.Create(value).IsSuccess)
            .WithMessage("Invalid current condition");
    }
}