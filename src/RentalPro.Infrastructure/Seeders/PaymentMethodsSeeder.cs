using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Payments;

namespace RentalPro.Infrastructure.Seeders;

public static class PaymentMethodSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.PaymentMethods.AnyAsync())
            return;

        var names = new[]
        {
            "Наличные",
            "Банковская карта"
        };

        foreach (var name in names)
        {
            var result = PaymentMethod.Create(name);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.PaymentMethods.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}