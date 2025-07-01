using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task<int> CreateProductWithOutbox(ProductDetails details, OutboxMessage outboxMessage);
    Task<(int productId, OutboxMessage outboxMessage)> CreateProductWithOutboxCallback(ProductDetails details, Func<int, OutboxMessage> createOutboxMessage);
    Task UpdateProductWithOutbox(int id, ProductDetails details, OutboxMessage outboxMessage);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    private DbSet<ProductDetailEntity> Products { get; set; } = null!;
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

    public async Task<int> CreateProductWithOutbox(ProductDetails details, OutboxMessage outboxMessage)
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
            await OutboxMessages.AddAsync(outboxMessage);
            await SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            return result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<(int productId, OutboxMessage outboxMessage)> CreateProductWithOutboxCallback(ProductDetails details, Func<int, OutboxMessage> createOutboxMessage)
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
            await SaveChangesAsync(); // Save first to get the ID
            
            var productId = result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");
            
            // Now create the outbox message with the actual product ID
            var outboxMessage = createOutboxMessage(productId);
            await OutboxMessages.AddAsync(outboxMessage);
            await SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            return (productId, outboxMessage);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProductWithOutbox(int id, ProductDetails details, OutboxMessage outboxMessage)
    {
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            var product = await Products.SingleAsync(a => a.Id == id);
            product.Description = details.Description;
            product.Name = details.Name;
            
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
    
    
    private class ProductDetailEntity
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}

