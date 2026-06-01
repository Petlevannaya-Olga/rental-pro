using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Payments;

public sealed class PaymentMethod : AuditableEntity<PaymentMethodId>
{
    private PaymentMethod(PaymentMethodName name)
        : base(PaymentMethodId.NewId())
    {
        Name = name;
    }

    public PaymentMethodName Name { get; private set; }

    public static Result<PaymentMethod, Error> Create(string name)
    {
        var nameResult = PaymentMethodName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new PaymentMethod(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = PaymentMethodName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(PaymentMethod));
    }
}