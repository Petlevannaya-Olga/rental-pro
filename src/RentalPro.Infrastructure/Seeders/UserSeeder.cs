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

        var adminRoleName = RoleName.Create("Администратор");

        if (adminRoleName.IsFailure)
            throw new InvalidOperationException(adminRoleName.Error.Message);

        var managerRoleName = RoleName.Create("Менеджер");

        if (managerRoleName.IsFailure)
            throw new InvalidOperationException(managerRoleName.Error.Message);

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == adminRoleName.Value);

        if (adminRole is null)
            throw new InvalidOperationException(
                "Role 'Администратор' was not found");

        var managerRole = await context.Roles
            .FirstOrDefaultAsync(x => x.Name == managerRoleName.Value);

        if (managerRole is null)
            throw new InvalidOperationException(
                "Role 'Менеджер' was not found");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        var adminUserResult = User.Create(
            login: "admin",
            passwordHash: passwordHash,
            lastName: "Иванов",
            firstName: "Иван",
            middleName: "Иванович",
            phoneNumber: "+79001112233",
            email: "admin@rentalpro.ru",
            roleId: adminRole.Id.Value);

        if (adminUserResult.IsFailure)
            throw new InvalidOperationException(
                adminUserResult.Error.Message);

        var managerUserResult = User.Create(
            login: "manager",
            passwordHash: passwordHash,
            lastName: "Петров",
            firstName: "Петр",
            middleName: "Петрович",
            phoneNumber: "+79002223344",
            email: "manager@rentalpro.ru",
            roleId: managerRole.Id.Value);

        if (managerUserResult.IsFailure)
            throw new InvalidOperationException(
                managerUserResult.Error.Message);

        await context.Users.AddRangeAsync(
            adminUserResult.Value,
            managerUserResult.Value);

        await context.SaveChangesAsync();
    }
}