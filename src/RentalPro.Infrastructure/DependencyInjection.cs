using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Application;
using RentalPro.Application.Auth;
using RentalPro.Application.Database;
using RentalPro.Infrastructure.Auth;
using RentalPro.Infrastructure.Database;
using RentalPro.Infrastructure.Repositories;

namespace RentalPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        
        services.AddScoped<IUserRepository, UsersRepository>();
        services.AddScoped<IUsersReadRepository, UsersReadRepository>();
        services.AddScoped<IRoleRepository, RolesRepository>();
        
        services.AddScoped<ITransactionManager, TransactionManager>();
        
        return services;
    }
}