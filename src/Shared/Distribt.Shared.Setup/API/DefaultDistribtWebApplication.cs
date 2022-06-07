using Distribt.Shared.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Distribt.Shared.Setup.API;

public static class DefaultDistribtWebApplication
{
    public static WebApplication Create(string[] args, Action<WebApplicationBuilder>? webappBuilder = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHealthChecks();
        builder.Services.AddHealthChecksUI().AddInMemoryStorage();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddRouting(x => x.LowercaseUrls = true);
        builder.Services.AddSerializer();

        builder.Services.AddServiceDiscovery(builder.Configuration);
        builder.Services.AddSecretManager(builder.Configuration);
        builder.Services.AddLogging(logger => logger.AddSerilog());

        builder.Host.ConfigureSerilog(builder.Services.BuildServiceProvider().GetRequiredService<IServiceDiscovery>());

        if (webappBuilder != null)
        {
            webappBuilder.Invoke(builder);
        }

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
        
        //if i have this information normalized, and is always the same,
         //Should I build in memory the configuration section to save every app to have the
         //healthcheck section in the ui?
        webApp.UseHealthChecksUI(config =>
        {
            config.UIPath = "/health-ui";            
        });


        webApp.UseHttpsRedirection();
        webApp.UseAuthorization();
        webApp.MapControllers();
        webApp.Run();
    }
}