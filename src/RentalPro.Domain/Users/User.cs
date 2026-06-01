using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Roles;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Users;

public sealed class User : AuditableEntity<UserId>
{
    private User()
        : base(UserId.NewId())
    {
    }
    
    private User(
        Login login,
        PasswordHash passwordHash,
        FullName fullName,
        PhoneNumber phoneNumber,
        Email email,
        RoleId roleId)
        : base(UserId.NewId())
    {
        Login = login;
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Email = email;
        RoleId = roleId;
        IsActive = true;
    }

    public Login Login { get; private set; }

    public PasswordHash PasswordHash { get; private set; }

    public FullName FullName { get; private set; }

    public PhoneNumber PhoneNumber { get; private set; }

    public Email Email { get; private set; }

    public RoleId RoleId { get; private set; }
    
    public Role Role { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static Result<User, Error> Create(
        string login,
        string passwordHash,
        string lastName,
        string firstName,
        string middleName,
        string phoneNumber,
        string email,
        Guid roleId)
    {
        var loginResult = Login.Create(login);

        if (loginResult.IsFailure)
            return loginResult.Error;

        var passwordHashResult = PasswordHash.Create(passwordHash);

        if (passwordHashResult.IsFailure)
            return passwordHashResult.Error;

        var fullNameResult = FullName.Create(
            lastName,
            firstName,
            middleName);

        if (fullNameResult.IsFailure)
            return fullNameResult.Error;

        var phoneNumberResult = PhoneNumber.Create(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Error;

        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
            return emailResult.Error;

        var roleIdResult = RoleId.Create(roleId);

        if (roleIdResult.IsFailure)
            return roleIdResult.Error;

        return new User(
            loginResult.Value,
            passwordHashResult.Value,
            fullNameResult.Value,
            phoneNumberResult.Value,
            emailResult.Value,
            roleIdResult.Value);
    }

    public UnitResult<Error> UpdateProfile(
        string lastName,
        string firstName,
        string middleName,
        string phoneNumber,
        string email)
    {
        var fullNameResult = FullName.Create(
            lastName,
            firstName,
            middleName);

        if (fullNameResult.IsFailure)
            return fullNameResult.Error;

        var phoneNumberResult = PhoneNumber.Create(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Error;

        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
            return emailResult.Error;

        FullName = fullNameResult.Value;
        PhoneNumber = phoneNumberResult.Value;
        Email = emailResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeRole(Guid roleId)
    {
        var roleIdResult = RoleId.Create(roleId);

        if (roleIdResult.IsFailure)
            return roleIdResult.Error;

        RoleId = roleIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangePasswordHash(string passwordHash)
    {
        var passwordHashResult = PasswordHash.Create(passwordHash);

        if (passwordHashResult.IsFailure)
            return passwordHashResult.Error;

        PasswordHash = passwordHashResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Activate()
    {
        if (DeletedAt.HasValue)
        {
            return CommonErrors.Validation(
                nameof(DeletedAt),
                "Deleted user cannot be activated");
        }

        IsActive = true;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Deactivate()
    {
        IsActive = false;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        IsActive = false;

        return MarkDeleted(nameof(User));
    }
}