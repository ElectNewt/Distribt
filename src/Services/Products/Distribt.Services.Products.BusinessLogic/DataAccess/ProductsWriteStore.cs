using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using Distribt.Shared.Outbox;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    public DbSet<ProductDetailEntity> Products { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public ProductsWriteStore(DbContextOptions<ProductsWriteStore> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessages");
        modelBuilder.Entity<OutboxMessage>().HasKey(o => o.Id);
        modelBuilder.Entity<OutboxMessage>().Property(o => o.Content).HasColumnType("json");

        modelBuilder.Entity<ProductDetailEntity>().ToTable("Products");
        modelBuilder.Entity<ProductDetailEntity>().HasKey(p => p.Id);

        base.OnModelCreating(modelBuilder);
    }

    public async Task UpdateProduct(int id, ProductDetails details)
    {
        using (var transaction = await Database.BeginTransactionAsync())
        {
            try
            {
                var product = await Products.SingleAsync(a => a.Id == id);
                product.Description = details.Description;
                product.Name = details.Name;

                await SaveChangesAsync();

                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventId = Guid.NewGuid(),
                    MessageType = "ProductUpdated",
                    Content = System.Text.Json.JsonSerializer.Serialize(new { ProductId = id, details.Name, details.Description }),
                    OccurredOn = DateTime.UtcNow
                };
                OutboxMessages.Add(outboxMessage);
                await SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<int> CreateRecord(ProductDetails details)
    {
        using (var transaction = await Database.BeginTransactionAsync())
        {
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

                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventId = Guid.NewGuid(),
                    MessageType = "ProductCreated",
                    Content = System.Text.Json.JsonSerializer.Serialize(new { ProductId = productId, details.Name, details.Description }),
                    OccurredOn = DateTime.UtcNow
                };
                OutboxMessages.Add(outboxMessage);
                await SaveChangesAsync();

                await transaction.CommitAsync();

                return productId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }


    public class ProductDetailEntity
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}

