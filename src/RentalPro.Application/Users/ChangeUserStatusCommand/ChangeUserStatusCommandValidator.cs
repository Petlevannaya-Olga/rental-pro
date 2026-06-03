using FluentValidation;

namespace RentalPro.Application.Users.ChangeUserStatusCommand;

public sealed class ChangeUserStatusCommandValidator
    : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull();
    }
}