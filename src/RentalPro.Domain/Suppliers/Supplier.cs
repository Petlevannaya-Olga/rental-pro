using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Suppliers;

public sealed class Supplier : AuditableEntity<SupplierId>
{
    private Supplier()
        : base(SupplierId.NewId())
    {
        Name = null!;
    }

    private Supplier(
        SupplierName name,
        PhoneNumber? phoneNumber,
        Email? email,
        Address? address,
        FullName? contactPerson)
        : base(SupplierId.NewId())
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        ContactPerson = contactPerson;
    }

    public SupplierName Name { get; private set; }

    public PhoneNumber? PhoneNumber { get; private set; }

    public Email? Email { get; private set; }

    public Address? Address { get; private set; }

    public FullName? ContactPerson { get; private set; }

    public static Result<Supplier, Error> Create(
        string name,
        string? phoneNumber,
        string? email,
        Address? address,
        string? contactLastName,
        string? contactFirstName,
        string? contactMiddleName)
    {
        var nameResult = SupplierName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var phoneNumberResult = CreatePhoneNumber(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Error;

        var emailResult = CreateEmail(email);

        if (emailResult.IsFailure)
            return emailResult.Error;

        var contactPersonResult = CreateContactPerson(
            contactLastName,
            contactFirstName,
            contactMiddleName);

        if (contactPersonResult.IsFailure)
            return contactPersonResult.Error;

        return new Supplier(
            nameResult.Value,
            phoneNumberResult.Value,
            emailResult.Value,
            address,
            contactPersonResult.Value);
    }

    public UnitResult<Error> Update(
        string name,
        string? phoneNumber,
        string? email,
        Address? address,
        string? contactLastName,
        string? contactFirstName,
        string? contactMiddleName)
    {
        var nameResult = SupplierName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var phoneNumberResult = CreatePhoneNumber(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Error;

        var emailResult = CreateEmail(email);

        if (emailResult.IsFailure)
            return emailResult.Error;

        var contactPersonResult = CreateContactPerson(
            contactLastName,
            contactFirstName,
            contactMiddleName);

        if (contactPersonResult.IsFailure)
            return contactPersonResult.Error;

        Name = nameResult.Value;
        PhoneNumber = phoneNumberResult.Value;
        Email = emailResult.Value;
        Address = address;
        ContactPerson = contactPersonResult.Value;

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
        return MarkDeleted(nameof(Supplier));
    }

    private static Result<PhoneNumber?, Error> CreatePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (PhoneNumber?)null;

        var phoneNumberResult = PhoneNumber.Create(phoneNumber);

        if (phoneNumberResult.IsFailure)
            return phoneNumberResult.Error;

        return phoneNumberResult.Value;
    }

    private static Result<Email?, Error> CreateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (Email?)null;

        var emailResult = Email.Create(email);

        if (emailResult.IsFailure)
            return emailResult.Error;

        return emailResult.Value;
    }

    private static Result<FullName?, Error> CreateContactPerson(
        string? lastName,
        string? firstName,
        string? middleName)
    {
        var allContactPersonFieldsAreEmpty =
            string.IsNullOrWhiteSpace(lastName)
            && string.IsNullOrWhiteSpace(firstName)
            && string.IsNullOrWhiteSpace(middleName);

        if (allContactPersonFieldsAreEmpty)
            return (FullName?)null;

        if (string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(middleName))
        {
            return CommonErrors.Validation(
                nameof(ContactPerson),
                "Contact person is incomplete");
        }

        var fullNameResult = FullName.Create(
            lastName,
            firstName,
            middleName);

        if (fullNameResult.IsFailure)
            return fullNameResult.Error;

        return fullNameResult.Value;
    }
}