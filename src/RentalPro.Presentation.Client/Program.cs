using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RentalPro.Presentation.Client;
using RentalPro.Presentation.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7099/")
    });
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<RolesService>();
builder.Services.AddScoped<FakeUserGeneratorService>();
builder.Services.AddScoped<DictionariesService>();
builder.Services.AddScoped<PaymentMethodsService>();
builder.Services.AddScoped<PaymentTypesService>();
builder.Services.AddScoped<OrderStatusesService>();
builder.Services.AddScoped<ToolCategoriesService>();
builder.Services.AddScoped<DictionaryCrudService>();
builder.Services.AddScoped<ToolsService>();
builder.Services.AddScoped<FakeToolGeneratorService>();
builder.Services.AddScoped<CustomersService>();
builder.Services.AddScoped<FakeCustomerGeneratorService>();

await builder.Build().RunAsync();