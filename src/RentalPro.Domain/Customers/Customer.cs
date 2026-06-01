using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Customers;

public sealed class Customer : AuditableEntity<CustomerId>
{
    // EF Core
    private Customer()
        : base(CustomerId.NewId())
    {
    }
    
    private Customer(
        FullName fullName,
        PhoneNumber phoneNumber,
        Email email,
        PassportData passportData,
        Address? address)
        : base(CustomerId.NewId())
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Email = email;
        PassportData = passportData;
        Address = address;
    }

    public FullName FullName { get; private set; }

    public PhoneNumber PhoneNumber { get; private set; }

    public Email Email { get; private set; }

    public PassportData PassportData { get; private set; }

    public Address? Address { get; private set; }

    public static Result<Customer, Error> Create(
        string lastName,
        string firstName,
        string middleName,
        string phoneNumber,
        string email,
        string passportSeries,
        string passportNumber,
        Address? address)
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

        var passportResult = PassportData.Create(
            passportSeries,
            passportNumber);

        if (passportResult.IsFailure)
            return passportResult.Error;

        return new Customer(
            fullNameResult.Value,
            phoneNumberResult.Value,
            emailResult.Value,
            passportResult.Value,
            address);
    }

    public UnitResult<Error> UpdateProfile(
        string lastName,
        string firstName,
        string middleName,
        string phoneNumber,
        string email,
        Address? address)
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
        Address = address;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangePassport(
        string passportSeries,
        string passportNumber)
    {
        var passportResult = PassportData.Create(
            passportSeries,
            passportNumber);

        if (passportResult.IsFailure)
            return passportResult.Error;

        PassportData = passportResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeAddress(Address? address)
    {
        Address = address;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Customer));
    }
}