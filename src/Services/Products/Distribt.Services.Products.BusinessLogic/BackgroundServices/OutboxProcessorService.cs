using Distribt.Services.Products.BusinessLogic.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Distribt.Services.Products.BusinessLogic.BackgroundServices;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30); // Process every 30 seconds

    public OutboxProcessorService(IServiceProvider serviceProvider, ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxMessageService = scope.ServiceProvider.GetRequiredService<IOutboxMessageService>();

        await outboxMessageService.ProcessPendingMessagesAsync(cancellationToken);
    }
} 