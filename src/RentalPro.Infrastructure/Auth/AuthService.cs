using Microsoft.EntityFrameworkCore;
using RentalPro.Application.Auth;
using RentalPro.Contracts.Auth;
using RentalPro.Domain.Users;

namespace RentalPro.Infrastructure.Auth;

public sealed class AuthService(
    ApplicationDbContext context,
    ITokenService tokenService)
    : IAuthService
{
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var loginResult = Login.Create(request.Login);

        if (loginResult.IsFailure)
            return LoginResponse.Fail("Неверный логин или пароль");

        var login = loginResult.Value;

        var user = await context.Users
            .FirstOrDefaultAsync(
                x => x.Login == login,
                cancellationToken);

        if (user is null)
            return LoginResponse.Fail("Неверный логин или пароль");

        if (!user.IsActive)
            return LoginResponse.Fail("Пользователь заблокирован");

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash.Value);

        if (!passwordValid)
            return LoginResponse.Fail("Неверный логин или пароль");

        var roleName = await context.Roles
                           .Where(x => x.Id == user.RoleId)
                           .Select(x => x.Name.Value)
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? "User";

        var fullName =
            $"{user.FullName.LastName} " +
            $"{user.FullName.FirstName} " +
            $"{user.FullName.MiddleName}";

        var token = tokenService.CreateToken(
            user.Id.Value,
            user.Login.Value,
            fullName,
            roleName);

        return LoginResponse.Ok(
            token,
            fullName,
            roleName);
    }
}