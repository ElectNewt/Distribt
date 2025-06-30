using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task AddOutboxMessage(string eventType, object eventData, string routingKey);
    Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, object eventData, string routingKey);
    Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey);
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

    public async Task AddOutboxMessage(string eventType, object eventData, string routingKey)
    {
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

    public async Task<int> CreateRecordWithOutboxMessage(ProductDetails details, string eventType, object eventData, string routingKey)
    {
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

    public async Task UpdateProductWithOutboxMessage(int id, ProductDetails details, string eventType, object eventData, string routingKey)
    {
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

