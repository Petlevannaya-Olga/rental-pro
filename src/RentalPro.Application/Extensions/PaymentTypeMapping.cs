using RentalPro.Contracts.PaymentTypes;
using RentalPro.Domain.Payments;

namespace RentalPro.Application.Extensions;

public static class PaymentTypeMapping
{
    public static PaymentTypeDto ToDto(this PaymentType method)
    {
        return new PaymentTypeDto(method.Id.Value, method.Name.Value, method.CreatedAt, method.UpdatedAt);
    }
}