using Distribt.Shared.Communication.Publisher.Domain;
using Distribt.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public class OutboxMessageService : IOutboxMessageService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly ISerializer _serializer;
    private readonly ILogger<OutboxMessageService> _logger;

    public OutboxMessageService(
        IOutboxRepository outboxRepository,
        IDomainMessagePublisher domainMessagePublisher,
        ISerializer serializer,
        ILogger<OutboxMessageService> logger)
    {
        _outboxRepository = outboxRepository;
        _domainMessagePublisher = domainMessagePublisher;
        _serializer = serializer;
        _logger = logger;
    }



    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unprocessedMessages = await _outboxRepository.GetUnprocessedMessagesAsync(10, cancellationToken);

        foreach (var message in unprocessedMessages)
        {
            try
            {
                await PublishMessage(message, cancellationToken);
                await _outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
                
                _logger.LogInformation("Successfully processed outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {EventType}. Retry count: {RetryCount}", 
                    message.Id, message.EventType, message.RetryCount);
                
                await _outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }

    private async Task PublishMessage(OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventData = DeserializeEventData(message);
        await _domainMessagePublisher.Publish(eventData, routingKey: message.RoutingKey, cancellationToken: cancellationToken);
    }

    private object DeserializeEventData(OutboxMessage message)
    {
        // Map event type names to actual types and deserialize using reflection
        return message.EventType switch
        {
            "ProductCreated" => _serializer.DeserializeObject<Distribt.Services.Products.Dtos.ProductCreated>(message.EventData),
            "ProductUpdated" => _serializer.DeserializeObject<Distribt.Services.Products.Dtos.ProductUpdated>(message.EventData),
            _ => throw new InvalidOperationException($"Unknown event type: {message.EventType}")
        };
    }
} 