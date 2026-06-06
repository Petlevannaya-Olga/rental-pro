using FluentValidation;
using RentalPro.Domain.Roles;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Roles.DeleteRoleCommand;

public sealed class DeleteRoleCommandValidator
    : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(RoleId.Create);
    }
}