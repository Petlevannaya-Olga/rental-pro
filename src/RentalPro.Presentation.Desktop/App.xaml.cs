using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Auth;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.ViewModels;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        const string apiBaseUrl = "https://localhost:7099/";

        services.AddSingleton<TokenStorage>();
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient<AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        services.AddHttpClient("Api", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddSingleton<NavigationService>();
        
        services.AddTransient<DashboardApiClient>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardView>();
        
        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        
        services.AddTransient<CustomersApiClient>();
        services.AddTransient<CustomersView>();
        services.AddTransient<CustomersViewModel>();
        services.AddSingleton<CustomerEditViewModel>();
        services.AddTransient<CustomerEditView>();
        
        services.AddSingleton<FakeCustomerGeneratorService>();
        
        services.AddSingleton<NotificationService>();
        services.AddSingleton<NotificationViewModel>();
        
        services.AddSingleton<CustomerOrderHistoryViewModel>();
        services.AddSingleton<CustomerOrderHistoryView>();
        
        services.AddSingleton<DictionariesApiClient>();
        
        services.AddSingleton<ToolsApiClient>();
        services.AddSingleton<FakeToolGeneratorService>();

        services.AddSingleton<ToolsViewModel>();
        services.AddSingleton<ToolsView>();
        
        services.AddSingleton<ToolEditViewModel>();
        services.AddSingleton<ToolEditView>();
        
        services.AddSingleton<ToolRentalHistoryViewModel>();
        services.AddSingleton<ToolRentalHistoryView>();
    }
}