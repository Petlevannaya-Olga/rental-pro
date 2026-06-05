using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Tools;

namespace RentalPro.Infrastructure.Seeders;

public static class ToolCategoriesSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ToolCategories.AnyAsync())
            return;

        var names = new[]
        {
            "Перфораторы",
            "Шуруповерты",
            "Дрели",
            "Болгарки",
            "Лобзики",
            "Пилы",
            "Шлифовальные машины",
            "Измерительные инструменты",
            "Сварочное оборудование",
            "Садовая техника"
        };

        foreach (var name in names)
        {
            var result = ToolCategory.Create(name);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.ToolCategories.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}