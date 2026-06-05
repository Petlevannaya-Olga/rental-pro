using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.ToolStatuses.DeleteToolStatusCommand;

public sealed class DeleteToolStatusCommandValidator
    : AbstractValidator<DeleteToolStatusCommand>
{
    public DeleteToolStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolStatusId.Create);
    }
}