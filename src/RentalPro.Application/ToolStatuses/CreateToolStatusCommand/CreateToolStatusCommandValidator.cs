using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolStatuses.CreateToolStatusCommand;

public sealed class CreateToolStatusCommandValidator
    : AbstractValidator<CreateToolStatusCommand>
{
    public CreateToolStatusCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(ToolStatusName.Create);
    }
}