using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Distribt.Shared.Setup.Observability;

public static class OpenTelemetry
{
    private static string? _openTelemetryUrl;
    
    public static void AddTracing(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOpenTelemetryTracing(builder => builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration["AppName"]))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(exporter =>
            { 
                string url = GetOpenTelemetryCollectorUrl(serviceCollection.BuildServiceProvider()).Result;
                exporter.Endpoint = new Uri(url);
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
                string url = GetOpenTelemetryCollectorUrl(serviceCollection.BuildServiceProvider()).Result;
                exporter.Endpoint = new Uri(url);
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

    private static async Task<string> GetOpenTelemetryCollectorUrl(IServiceProvider serviceProvider)
    {
        if (_openTelemetryUrl != null)
            return _openTelemetryUrl;
        
        
        var serviceDiscovery = serviceProvider.GetService<IServiceDiscovery>();
        string openTelemetryLocation = await serviceDiscovery?.GetFullAddress(DiscoveryServices.OpenTelemetry)!;
        _openTelemetryUrl = $"http://{openTelemetryLocation}";
        return _openTelemetryUrl;
    }
    
}