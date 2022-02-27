using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace Distribt.Shared.Logging.Loggers;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration AddConsoleLogger(this LoggerConfiguration loggerConfiguration,
        ConsoleLoggerConfiguration consoleLoggerConfiguration)
    {
        return consoleLoggerConfiguration.Enabled
            ? loggerConfiguration.WriteTo.Console(consoleLoggerConfiguration.MinimumLevel)
            : loggerConfiguration;
    }

    public static LoggerConfiguration AddGraylogLogger(this LoggerConfiguration loggerConfiguration,
        GraylogLoggerConfiguration graylogLoggerConfiguration)
    {
        return graylogLoggerConfiguration.Enabled
            ? loggerConfiguration.WriteTo.Graylog(graylogLoggerConfiguration.Host, graylogLoggerConfiguration.Port, 
                TransportType.Udp, graylogLoggerConfiguration.MinimumLevel)
            : loggerConfiguration;
    }
    
}