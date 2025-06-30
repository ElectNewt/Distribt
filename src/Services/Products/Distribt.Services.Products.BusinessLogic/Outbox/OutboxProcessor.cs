using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Distribt.Services.Products.BusinessLogic.Outbox;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetUnprocessedMessages(int batchSize = 100);
    Task MarkAsProcessed(int messageId);
    Task MarkAsFailed(int messageId, string errorMessage);
}

public class OutboxRepository : IOutboxRepository
{
    private readonly ProductsWriteStore _dbContext;

    public OutboxRepository(ProductsWriteStore dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessages(int batchSize = 100)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkAsProcessed(int messageId)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkAsFailed(int messageId, string errorMessage)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync(messageId);
        if (message != null)
        {
            message.RetryCount++;
            message.ErrorMessage = errorMessage;
            if (message.RetryCount >= 3)
            {
                message.IsProcessed = true; // Mark as processed to stop retrying
                message.ProcessedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        IDomainMessagePublisher domainMessagePublisher,
        ILogger<OutboxProcessor> logger)
    {
        _outboxRepository = outboxRepository;
        _domainMessagePublisher = domainMessagePublisher;
        _logger = logger;
    }

    public async Task ProcessPendingMessages(CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = await _outboxRepository.GetUnprocessedMessages();
            
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
                        await _outboxRepository.MarkAsFailed(message.Id, $"Unknown event type: {message.EventType}");
                        continue;
                    }

                    var eventData = System.Text.Json.JsonSerializer.Deserialize(message.EventData, eventType);
                    if (eventData == null)
                    {
                        _logger.LogError("Failed to deserialize event data for message {MessageId}", message.Id);
                        await _outboxRepository.MarkAsFailed(message.Id, "Failed to deserialize event data");
                        continue;
                    }

                    await _domainMessagePublisher.Publish(eventData, routingKey: message.RoutingKey);
                    await _outboxRepository.MarkAsProcessed(message.Id);
                    
                    _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    await _outboxRepository.MarkAsFailed(message.Id, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox messages");
        }
    }
}