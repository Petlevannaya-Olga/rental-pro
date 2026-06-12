using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Application;
using RentalPro.Application.Auth;
using RentalPro.Application.Database;
using RentalPro.Application.Files;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Contracts.Customers;
using RentalPro.Contracts.Orders;
using RentalPro.Contracts.Users;
using RentalPro.Infrastructure.Auth;
using RentalPro.Infrastructure.Database;
using RentalPro.Infrastructure.Files;
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
        services.AddScoped<IToolsRepository, ToolsRepository>();
        services.AddScoped<IToolsReadRepository, ToolsReadRepository>();
        services.AddScoped<ICustomersReadRepository, CustomersReadRepository>();
        services.AddScoped<ICustomersRepository, CustomersRepository>();
        services.AddScoped<IOrdersReadRepository, OrdersReadRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        
        services.AddScoped<ITransactionManager, TransactionManager>();
        
        services.AddScoped<IExcelExportService<UserDto>, UsersExportService>();
        services.AddScoped<IExcelExportService<CustomerDto>, CustomersExportService>();
        services.AddScoped<IExcelExportService<OrderDto>, OrdersExportService>();
        
        services.AddScoped<IContractDocumentService, ContractDocumentService>();
        services.AddScoped<IContractPdfService, ContractPdfService>();
        services.AddScoped<ITransferActDocumentService, TransferActDocumentService>();
        
        return services;
    }
}