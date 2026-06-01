using Microsoft.OpenApi;
using RentalPro.Infrastructure;
using RentalPro.Shared;

namespace RentalPro.Presentation.Server.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSerilogLogging(configuration)
            //.AddWebDependencies()
            //.AddApplication()
            .AddInfrastructure(configuration);

        return services;
    }

    /*private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type != typeof(Envelope<Errors>))
                {
                    return Task.CompletedTask;
                }

                if (schema.Properties.TryGetValue("errors", out OpenApiSchema? errorsProperty))
                {
                    errorsProperty.Items.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema, Id = "Error",
                    };
                }

                return Task.CompletedTask;
            });
        });
        return services;
    }*/

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        /*services.AddSerilog((s, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(s)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"));
*/
        return services;
    }
}