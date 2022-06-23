using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Distribt.Shared.Setup.Observability;

public static class OpenTelemetry
{
    public static void AddTracing(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOpenTelemetryTracing(builder => builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration["AppName"]))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(exporter =>
            {
                //TODO: call the discovery service to retrieve the correctUrl dinamically
                exporter.Endpoint = new Uri("http://localhost:4317");
            })
        );
        ;
    }

    public static void AddMetrics(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOpenTelemetryMetrics(builder => builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration["AppName"]))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(exporter =>
            {
                //TODO: call the discovery service to retrieve the correctUrl dinamically
                exporter.Endpoint = new Uri("http://localhost:4317");
            }));
    }
    
    // Not used in distribt, added here because of the blogpost.
    public static void AddLogging(this IHostBuilder builder, IConfiguration configuration)
    {
        builder.ConfigureLogging(logging => logging
                //Next line optional to remove other providers
                .ClearProviders()
                .AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration["AppName"]));
                    options.AddConsoleExporter();
                }));
    }
    
    
}