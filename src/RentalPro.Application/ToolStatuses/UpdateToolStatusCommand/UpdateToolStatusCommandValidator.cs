using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolStatuses.UpdateToolStatusCommand;

public sealed class UpdateToolStatusCommandValidator
    : AbstractValidator<UpdateToolStatusCommand>
{
    public UpdateToolStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolStatusId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(ToolStatusName.Create);
    }
}