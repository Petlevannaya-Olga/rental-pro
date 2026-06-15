using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Payments;

namespace RentalPro.Infrastructure.Seeders;

public static class PaymentTypeSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.PaymentTypes.AnyAsync())
            return;

        var names = new[]
        {
            "Аренда",
            "Залог",
            "Возврат залога",
            "Возврат аренды"
        };

        foreach (var name in names)
        {
            var result = PaymentType.Create(name);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.PaymentTypes.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}