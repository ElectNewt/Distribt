using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.Services;

public interface IOutboxMessageProcessor
{
    Task ProcessOutboxMessages(CancellationToken cancellationToken = default);
}

public class OutboxMessageProcessor : BackgroundService, IOutboxMessageProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public OutboxMessageProcessor(IServiceProvider serviceProvider, ILogger<OutboxMessageProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
    }

    public async Task ProcessOutboxMessages(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var writeStore = scope.ServiceProvider.GetRequiredService<IProductsWriteStore>();
        var domainMessagePublisher = scope.ServiceProvider.GetRequiredService<IDomainMessagePublisher>();

        var unprocessedMessages = await writeStore.GetUnprocessedOutboxMessages();

        foreach (var message in unprocessedMessages)
        {
            try
            {
                await PublishMessage(message, domainMessagePublisher, cancellationToken);
                await writeStore.MarkOutboxMessageAsProcessed(message.Id);
                
                _logger.LogInformation("Successfully processed outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);
                // Continue processing other messages even if one fails
            }
        }
    }

    private async Task PublishMessage(OutboxMessage message, IDomainMessagePublisher domainMessagePublisher, CancellationToken cancellationToken)
    {
        switch (message.EventType)
        {
            case nameof(ProductCreated):
                var productCreatedEvent = JsonSerializer.Deserialize<ProductCreated>(message.EventData);
                if (productCreatedEvent != null)
                {
                    await domainMessagePublisher.Publish(productCreatedEvent, metadata: null, routingKey: "internal");
                }
                break;

            case nameof(ProductUpdated):
                var productUpdatedEvent = JsonSerializer.Deserialize<ProductUpdated>(message.EventData);
                if (productUpdatedEvent != null)
                {
                    await domainMessagePublisher.Publish(productUpdatedEvent, metadata: null, routingKey: "internal");
                }
                break;

            default:
                _logger.LogWarning("Unknown event type {EventType} for outbox message {MessageId}", 
                    message.EventType, message.Id);
                break;
        }
    }
}
