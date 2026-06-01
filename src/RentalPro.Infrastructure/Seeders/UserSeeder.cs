using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;

namespace RentalPro.Infrastructure.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var roleResult = Role.Create("Администратор");

        if (roleResult.IsFailure)
            throw new InvalidOperationException(roleResult.Error.Message);

        var role = roleResult.Value;

        await context.Roles.AddAsync(role);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        var userResult = User.Create(
            login: "admin",
            passwordHash: passwordHash,
            lastName: "Иванов",
            firstName: "Иван",
            middleName: "Иванович",
            phoneNumber: "+79001112233",
            email: "admin@rentalpro.ru",
            roleId: role.Id.Value);

        if (userResult.IsFailure)
            throw new InvalidOperationException(userResult.Error.Message);

        await context.Users.AddAsync(userResult.Value);

        await context.SaveChangesAsync();
    }
}