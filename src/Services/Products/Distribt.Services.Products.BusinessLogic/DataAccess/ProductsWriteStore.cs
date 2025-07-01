using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task SaveOutboxMessage(object eventData, string eventType);
    Task<List<OutboxMessage>> GetUnprocessedOutboxMessages();
    Task MarkOutboxMessageAsProcessed(int outboxMessageId);
    Task ExecuteInTransaction(Func<Task> operation);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    public DbSet<ProductDetailEntity> Products { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public ProductsWriteStore(DbContextOptions<ProductsWriteStore> options) : base(options)
    {
    }
    public async Task UpdateProduct(int id, ProductDetails details)
    {
        var product = await Products.SingleAsync(a => a.Id == id);
        product.Description = details.Description;
        product.Name = details.Name;
        
        await SaveChangesAsync();
    }

    public async Task<int> CreateRecord(ProductDetails details)
    {
        ProductDetailEntity newProduct = new ProductDetailEntity()
        {
            Description = details.Description,
            Name = details.Name
        };
        
        var result = await Products.AddAsync(newProduct);
        await SaveChangesAsync();
        
        return result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");
    }

    public async Task SaveOutboxMessage(object eventData, string eventType)
    {
        var outboxMessage = new OutboxMessage
        {
            EventType = eventType,
            EventData = JsonSerializer.Serialize(eventData),
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await OutboxMessages.AddAsync(outboxMessage);
        await SaveChangesAsync();
    }

    public async Task<List<OutboxMessage>> GetUnprocessedOutboxMessages()
    {
        return await OutboxMessages
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkOutboxMessageAsProcessed(int outboxMessageId)
    {
        var message = await OutboxMessages.FindAsync(outboxMessageId);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await SaveChangesAsync();
        }
    }

    public async Task ExecuteInTransaction(Func<Task> operation)
    {
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    
    public class ProductDetailEntity
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}

public class OutboxMessage
{
    public int Id { get; set; }
    public string EventType { get; set; } = null!;
    public string EventData { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

