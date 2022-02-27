using Distribt.Shared.Logging.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Distribt.Shared.Logging;

public static class ConfigureLogger
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
        => builder.UseSerilog((context, loggerConfiguration)
            => ConfigureSerilogLogger(loggerConfiguration, context.Configuration));

    private static LoggerConfiguration ConfigureSerilogLogger(LoggerConfiguration loggerConfiguration,
        IConfiguration configuration)
    {
        GraylogLoggerConfiguration graylogLogger = new GraylogLoggerConfiguration();
        configuration.GetSection("Logging:Graylog").Bind(graylogLogger);
        ConsoleLoggerConfiguration consoleLogger = new ConsoleLoggerConfiguration();
        configuration.GetSection("Logging:Console").Bind(consoleLogger);

        return loggerConfiguration
                .AddConsoleLogger(consoleLogger)
                .AddGraylogLogger(graylogLogger);
    }
}