namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public interface IOutboxRepository
{
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 10, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default);
    Task<int> GetUnprocessedMessageCountAsync(CancellationToken cancellationToken = default);
} 