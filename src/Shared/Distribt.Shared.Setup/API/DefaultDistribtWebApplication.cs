using Distribt.Shared.Logging;
using Distribt.Shared.Setup.Observability;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;


namespace Distribt.Shared.Setup.API;

public static class DefaultDistribtWebApplication
{
    public static async Task<WebApplication> Create(string[] args, Action<WebApplicationBuilder>? webappBuilder = null)
    {
        WebApplicationBuilder builder = await CreateBuilder(args);
        webappBuilder?.Invoke(builder);
        return builder.Build();
    }

    public static async Task<WebApplication> Create(string[] args, Func<WebApplicationBuilder, Task> webappBuilder)
    {
        WebApplicationBuilder builder = await CreateBuilder(args);
        await webappBuilder.Invoke(builder);
        return builder.Build();
    }

    public static void Run(WebApplication webApp)
    {
        if (webApp.Environment.IsDevelopment())
        {
            webApp.UseSwagger();
            webApp.UseSwaggerUI();
        }

        webApp.MapHealthChecks("/health");

        webApp.UseHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        webApp.UseHealthChecksUI(config =>
        {
            config.UIPath = "/health-ui";
        });


        webApp.UseHttpsRedirection();
        webApp.UseAuthorization();
        webApp.MapControllers();
        webApp.Run();
    }

    private static async Task<WebApplicationBuilder> CreateBuilder(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddConfiguration(HealthCheckHelper.BuildBasicHealthCheck());
        builder.Services.AddHealthChecks();
        builder.Services.AddHealthChecksUI().AddInMemoryStorage();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddRouting(x => x.LowercaseUrls = true);
        builder.Services.AddSerializer();
        builder.Services.AddServiceDiscovery(builder.Configuration);
        await builder.Services.AddSecretManager(builder.Configuration);
        builder.Services.AddLogging(logger => logger.AddSerilog());
        builder.Services.AddTracing(builder.Configuration);
        builder.Services.AddMetrics(builder.Configuration);

        builder.Host.ConfigureSerilog(builder.Services.BuildServiceProvider().GetRequiredService<IServiceDiscovery>());
        return builder;
    }
}