using FluentValidation;
using RentalPro.Domain.Tools;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Tools.DeleteToolCommand;

public sealed class DeleteToolCommandValidator
    : AbstractValidator<DeleteToolCommand>
{
    public DeleteToolCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(ToolId.Create);
    }
}