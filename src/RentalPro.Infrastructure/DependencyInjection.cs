using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Application;
using RentalPro.Application.Auth;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Infrastructure.Auth;
using RentalPro.Infrastructure.Database;
using RentalPro.Infrastructure.Repositories;
using RentalPro.Infrastructure.Services;

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
        services.AddScoped<IPaymentMethodsRepository, PaymentMethodsRepository>();
        services.AddScoped<IPaymentTypesRepository, PaymentTypesRepository>();
        services.AddScoped<IOrderStatusesRepository, OrderStatusesRepository>();
        services.AddScoped<IToolStatusesRepository, ToolStatusesRepository>();
        services.AddScoped<IToolCategoriesRepository, ToolCategoriesRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IDictionaryStatsRepository, DictionaryStatsRepository>();
        
        services.AddScoped<ITransactionManager, TransactionManager>();
        
        services.AddScoped<IUsersExportService, UsersExportService>();
        
        return services;
    }
}