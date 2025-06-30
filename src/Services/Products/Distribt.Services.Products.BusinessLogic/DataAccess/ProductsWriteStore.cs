using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, object eventData, string routingKey);
    Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey);
    Task<List<OutboxMessage>> GetUnprocessedMessages(int batchSize = 100);
    Task MarkAsProcessed(int messageId);
    Task MarkAsFailed(int messageId, string errorMessage);
}

public class ProductDetailEntity
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
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
        
        return result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessages(int batchSize = 100)
    {
        return await OutboxMessages
            .Where(m => !m.IsProcessed && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkAsProcessed(int messageId)
    {
        var message = await OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
            await SaveChangesAsync();
        }
    }

    public async Task MarkAsFailed(int messageId, string errorMessage)
    {
        var message = await OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.RetryCount++;
            message.ErrorMessage = errorMessage;
            if (message.RetryCount >= 3)
            {
                message.IsProcessed = true; // Mark as processed to stop retrying
                message.ProcessedAt = DateTime.UtcNow;
            }
            await SaveChangesAsync();
        }
    }

    public async Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, object eventData, string routingKey)
    {
        // Check if we're using in-memory database (for tests)
        var isInMemoryDatabase = Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        
        if (isInMemoryDatabase)
        {
            // For in-memory database, we don't need transactions
            ProductDetailEntity newProduct = new ProductDetailEntity()
            {
                Description = details.Description,
                Name = details.Name
            };
            
            var result = await Products.AddAsync(newProduct);
            await SaveChangesAsync();

            var productId = result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");

            // Update the event data with the actual product ID if it's a ProductCreated event
            if (eventData is ProductCreated productCreated)
            {
                eventData = new ProductCreated(productId, productCreated.ProductRequest);
            }

            var outboxMessage = new OutboxMessage
            {
                EventType = eventType,
                EventData = JsonSerializer.Serialize(eventData),
                RoutingKey = routingKey,
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false,
                RetryCount = 0
            };

            await OutboxMessages.AddAsync(outboxMessage);
            await SaveChangesAsync();

            return productId;
        }
        else
        {
            // For real databases, use transactions
            using var transaction = await Database.BeginTransactionAsync();
            try
            {
                ProductDetailEntity newProduct = new ProductDetailEntity()
                {
                    Description = details.Description,
                    Name = details.Name
                };
                
                var result = await Products.AddAsync(newProduct);
                await SaveChangesAsync();

                var productId = result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");

                // Update the event data with the actual product ID if it's a ProductCreated event
                if (eventData is ProductCreated productCreated)
                {
                    eventData = new ProductCreated(productId, productCreated.ProductRequest);
                }

                var outboxMessage = new OutboxMessage
                {
                    EventType = eventType,
                    EventData = JsonSerializer.Serialize(eventData),
                    RoutingKey = routingKey,
                    CreatedAt = DateTime.UtcNow,
                    IsProcessed = false,
                    RetryCount = 0
                };

                await OutboxMessages.AddAsync(outboxMessage);
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
    }

    public async Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey)
    {
        // Check if we're using in-memory database (for tests)
        var isInMemoryDatabase = Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        
        if (isInMemoryDatabase)
        {
            // For in-memory database, we don't need transactions
            var product = await Products.SingleAsync(a => a.Id == id);
            product.Description = details.Description;
            product.Name = details.Name;

            var outboxMessage = new OutboxMessage
            {
                EventType = eventType,
                EventData = JsonSerializer.Serialize(eventData),
                RoutingKey = routingKey,
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false,
                RetryCount = 0
            };

            await OutboxMessages.AddAsync(outboxMessage);
            await SaveChangesAsync();
        }
        else
        {
            // For real databases, use transactions
            using var transaction = await Database.BeginTransactionAsync();
            try
            {
                var product = await Products.SingleAsync(a => a.Id == id);
                product.Description = details.Description;
                product.Name = details.Name;

                var outboxMessage = new OutboxMessage
                {
                    EventType = eventType,
                    EventData = JsonSerializer.Serialize(eventData),
                    RoutingKey = routingKey,
                    CreatedAt = DateTime.UtcNow,
                    IsProcessed = false,
                    RetryCount = 0
                };

                await OutboxMessages.AddAsync(outboxMessage);
                await SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}

