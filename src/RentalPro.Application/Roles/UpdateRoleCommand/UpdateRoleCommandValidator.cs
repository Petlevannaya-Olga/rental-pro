using FluentValidation;
using RentalPro.Domain.Roles;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Roles.UpdateRoleCommand;

public sealed class UpdateRoleCommandValidator
    : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(RoleId.Create);

        RuleFor(x => x.Name)
            .MustBeValueObject(RoleName.Create);
    }
}