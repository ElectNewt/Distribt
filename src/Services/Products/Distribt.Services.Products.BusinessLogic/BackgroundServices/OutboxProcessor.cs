using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Distribt.Services.Products.BusinessLogic.DataAccess;

namespace Distribt.Services.Products.BusinessLogic.BackgroundServices;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IProductsWriteStore>() as ProductsWriteStore;
                var publisher = scope.ServiceProvider.GetRequiredService<IDomainMessagePublisher>();
                if (context is null) { await Task.Delay(1000, stoppingToken); continue; }
                var pending = await context.GetPendingOutboxMessages(stoppingToken);
                foreach (var message in pending)
                {
                    var type = Type.GetType(message.Type);
                    if (type == null) continue;
                    var obj = System.Text.Json.JsonSerializer.Deserialize(message.Payload, type);
                    if (obj == null) continue;
                    await publisher.Publish(obj, routingKey: "internal", cancellationToken: stoppingToken);
                }
                await context.MarkAsSent(pending, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox");
            }
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
