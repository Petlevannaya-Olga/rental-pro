using FluentValidation;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Users.UpdateUserCommand;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .MustBeValueObject(UserId.Create);

        RuleFor(x => x.Login)
            .MustBeValueObject(Login.Create);

        RuleFor(x => x.LastName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(UpdateUserCommand.LastName),
                    value).IsSuccess)
            .WithMessage("Invalid last name");

        RuleFor(x => x.FirstName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(UpdateUserCommand.FirstName),
                    value).IsSuccess)
            .WithMessage("Invalid first name");

        RuleFor(x => x.MiddleName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(UpdateUserCommand.MiddleName),
                    value).IsSuccess)
            .WithMessage("Invalid middle name");

        RuleFor(x => x.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create);

        RuleFor(x => x.RoleId)
            .MustBeValueObject(RoleId.Create);
    }
}