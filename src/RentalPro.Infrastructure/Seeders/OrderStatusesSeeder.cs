using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Orders;

namespace RentalPro.Infrastructure.Seeders;

public static class OrderStatusesSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.OrderStatuses.AnyAsync())
            return;

        var names = new[]
        {
            "Подтвержден",
            "Выдан",
            "Готов к выдаче",
            "Возвращен",
            "Просрочен",
            "Отменен",
            "Выполняется",
            "Ожидает возврата залога",
            "Завершен"
        };

        foreach (var name in names)
        {
            var result = OrderStatus.Create(name);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.OrderStatuses.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}