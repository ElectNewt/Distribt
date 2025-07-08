using Distribt.Shared.Discovery;
using Distribt.Shared.Logging.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Distribt.Shared.Logging;

public static class ConfigureLogger
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder, IServiceDiscovery discovery)
    {
        return builder.UseSerilog(async (HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration loggerConfiguration) =>
        {
            await ConfigureSerilogLogger(loggerConfiguration, context.Configuration, discovery);
        });
    }

    private static async Task<LoggerConfiguration> ConfigureSerilogLogger(LoggerConfiguration loggerConfiguration,
        IConfiguration configuration, IServiceDiscovery discovery)
    {
        GraylogLoggerConfiguration graylogLogger = new GraylogLoggerConfiguration();
        configuration.GetSection("Logging:Graylog").Bind(graylogLogger);
        DiscoveryData discoveryData = await discovery.GetDiscoveryData(DiscoveryServices.Graylog);
        graylogLogger.Host = discoveryData.Server;
        graylogLogger.Port = discoveryData.Port;
        ConsoleLoggerConfiguration consoleLogger = new ConsoleLoggerConfiguration();
        configuration.GetSection("Logging:Console").Bind(consoleLogger);

        return loggerConfiguration
                .AddConsoleLogger(consoleLogger)
                .AddGraylogLogger(graylogLogger);
    }
}