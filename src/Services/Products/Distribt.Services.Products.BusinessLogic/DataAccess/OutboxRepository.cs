using Microsoft.EntityFrameworkCore;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public class OutboxRepository : IOutboxRepository
{
    private readonly ProductsWriteStore _context;

    public OutboxRepository(ProductsWriteStore context)
    {
        _context = context;
    }

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 10, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(m => !m.IsProcessed && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            message.ErrorMessage = null;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        if (message != null)
        {
            message.RetryCount++;
            message.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetUnprocessedMessageCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .CountAsync(m => !m.IsProcessed && m.RetryCount < 3, cancellationToken);
    }
} 