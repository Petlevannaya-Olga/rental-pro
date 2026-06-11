using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RentalPro.Presentation.Client;
using RentalPro.Presentation.Client.Providers;
using RentalPro.Presentation.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services
    .AddHttpClient("Api", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7099/");
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("Api"));


builder.Services.AddSingleton<NotificationService>();
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
builder.Services.AddScoped<FakeOrderGeneratorService>();
builder.Services.AddScoped<OrdersService>();

await builder.Build().RunAsync();