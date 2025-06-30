using Distribt.Services.Products.BusinessLogic.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Distribt.Services.Products.BusinessLogic.Services;

public class OutboxBackgroundService : BackgroundService
{
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);

    public OutboxBackgroundService(
        ILogger<OutboxBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                
                await outboxProcessor.ProcessPendingMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox background service");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox background service stopped");
    }
}