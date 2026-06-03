using FluentValidation;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Users.ChangeUserPasswordCommand;

public sealed class ChangeUserPasswordCommandValidator
    : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .MustBeValueObject(UserId.Create);

        RuleFor(x => x.OldPassword)
            .MustBeValueObject(Password.Create);

        RuleFor(x => x.NewPassword)
            .MustBeValueObject(Password.Create);
    }
}