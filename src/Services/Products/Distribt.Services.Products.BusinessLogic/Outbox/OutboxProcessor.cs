using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.Extensions.Logging;

namespace Distribt.Services.Products.BusinessLogic.Outbox;

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IProductsWriteStore _writeStore;
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IProductsWriteStore writeStore,
        IDomainMessagePublisher domainMessagePublisher,
        ILogger<OutboxProcessor> logger)
    {
        _writeStore = writeStore;
        _domainMessagePublisher = domainMessagePublisher;
        _logger = logger;
    }

    public async Task ProcessPendingMessages(CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = await _writeStore.GetUnprocessedMessages();
            
            foreach (var message in messages)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // Deserialize and publish the message
                    var eventType = Type.GetType(message.EventType);
                    if (eventType == null)
                    {
                        _logger.LogError("Unknown event type: {EventType}", message.EventType);
                        await _writeStore.MarkAsFailed(message.Id, $"Unknown event type: {message.EventType}");
                        continue;
                    }

                    var eventData = System.Text.Json.JsonSerializer.Deserialize(message.EventData, eventType);
                    if (eventData == null)
                    {
                        _logger.LogError("Failed to deserialize event data for message {MessageId}", message.Id);
                        await _writeStore.MarkAsFailed(message.Id, "Failed to deserialize event data");
                        continue;
                    }

                    await _domainMessagePublisher.Publish(eventData, routingKey: message.RoutingKey);
                    await _writeStore.MarkAsProcessed(message.Id);
                    
                    _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    await _writeStore.MarkAsFailed(message.Id, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox messages");
        }
    }
}