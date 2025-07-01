using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task AddOutboxMessage(string eventType, object eventData, string routingKey);
    Task<List<OutboxMessage>> GetUnprocessedOutboxMessages();
    Task MarkOutboxMessageAsProcessed(int messageId);
    Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, Func<int, object> eventDataFactory, string routingKey);
    Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    private DbSet<ProductDetailEntity> Products { get; set; } = null!;
    private DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

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
        
        return result.Entity.Id;
    }

    public async Task AddOutboxMessage(string eventType, object eventData, string routingKey)
    {
        var outboxMessage = new OutboxMessage
        {
            EventType = eventType,
            EventData = JsonSerializer.Serialize(eventData),
            RoutingKey = routingKey,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await OutboxMessages.AddAsync(outboxMessage);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedOutboxMessages()
    {
        return await OutboxMessages
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkOutboxMessageAsProcessed(int messageId)
    {
        var message = await OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await SaveChangesAsync();
        }
    }

    public async Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, Func<int, object> eventDataFactory, string routingKey)
    {
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            var productId = await CreateRecord(details);
            var eventData = eventDataFactory(productId);
            await AddOutboxMessage(eventType, eventData, routingKey);
            await SaveChangesAsync();
            await transaction.CommitAsync();
            return productId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey)
    {
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            await UpdateProduct(id, details);
            await AddOutboxMessage(eventType, eventData, routingKey);
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    private class ProductDetailEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

public class OutboxMessage
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

