using FluentValidation;
using RentalPro.Domain.Users;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Users.DeleteUserCommand;

public sealed class DeleteUserCommandValidator
    : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .MustBeValueObject(UserId.Create);
    }
}