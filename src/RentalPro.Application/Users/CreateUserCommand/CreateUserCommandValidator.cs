using FluentValidation;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared.Extensions;

namespace RentalPro.Application.Users.CreateUserCommand;

public sealed class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .MustBeValueObject(Login.Create);

        RuleFor(x => x.Password)
            .MustBeValueObject(Password.Create);

        RuleFor(x => x.LastName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(CreateUserCommand.LastName),
                    value).IsSuccess)
            .WithMessage("Invalid last name");

        RuleFor(x => x.FirstName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(CreateUserCommand.FirstName),
                    value).IsSuccess)
            .WithMessage("Invalid first name");

        RuleFor(x => x.MiddleName)
            .Must(value =>
                FullName.ValidatePart(
                    nameof(CreateUserCommand.MiddleName),
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