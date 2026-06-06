using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Roles;

namespace RentalPro.Infrastructure.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new[]
        {
            "Администратор",
            "Менеджер"
        };

        foreach (var roleName in roles)
        {
            var result = Role.Create(roleName);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.Roles.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }
}