using RentalPro.Contracts.PaymentMethods;
using RentalPro.Domain.Payments;

namespace RentalPro.Application.Extensions;

public static class PaymentMethodMappings
{
    public static PaymentMethodDto ToDto(this PaymentMethod method)
    {
        return new PaymentMethodDto(method.Id.Value, method.Name.Value, method.CreatedAt, method.UpdatedAt);
    }
}