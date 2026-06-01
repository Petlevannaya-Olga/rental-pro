using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public sealed class PaymentType : AuditableEntity<PaymentTypeId>
{
    private PaymentType(PaymentTypeName name)
        : base(PaymentTypeId.NewId())
    {
        Name = name;
    }

    public PaymentTypeName Name { get; private set; }

    public static Result<PaymentType, Error> Create(string name)
    {
        var nameResult = PaymentTypeName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new PaymentType(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = PaymentTypeName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(PaymentType));
    }
}