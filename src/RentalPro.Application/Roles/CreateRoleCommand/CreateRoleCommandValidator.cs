using FluentValidation;
using RentalPro.Domain.Roles;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Roles.CreateRoleCommand;

public sealed class CreateRoleCommandValidator
    : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(RoleName.Create);
    }
}