using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Manufacturers;

namespace RentalPro.Infrastructure.Seeders;

public static class ManufacturersSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Manufacturers.AnyAsync())
            return;

        var manufacturers = new[]
        {
            ("Bosch", "Германия"),
            ("Makita", "Япония"),
            ("DeWalt", "США"),
            ("Milwaukee", "США"),
            ("Metabo", "Германия"),
            ("Hilti", "Лихтенштейн"),
            ("Ryobi", "Япония"),
            ("Patriot", "США"),
            ("Зубр", "Россия"),
            ("Интерскол", "Россия")
        };

        foreach (var (name, country) in manufacturers)
        {
            var result = Manufacturer.Create(
                name,
                country);

            if (result.IsFailure)
                throw new InvalidOperationException(
                    result.Error.Message);

            await context.Manufacturers.AddAsync(
                result.Value);
        }

        await context.SaveChangesAsync();
    }
}