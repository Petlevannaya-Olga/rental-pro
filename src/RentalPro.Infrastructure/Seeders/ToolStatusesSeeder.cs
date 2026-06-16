using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Tools;

namespace RentalPro.Infrastructure.Seeders;

public static class ToolStatusesSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ToolStatuses.AnyAsync())
            return;

        var names = new[]
        {
            "Доступен",
            "В аренде",
            "На ремонте",
            "Забронирован",
            "Списан"
        };

        foreach (var name in names)
        {
            var result = ToolStatus.Create(name);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.ToolStatuses.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}