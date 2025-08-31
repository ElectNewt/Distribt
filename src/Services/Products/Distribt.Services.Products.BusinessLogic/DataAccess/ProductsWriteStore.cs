using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    private DbSet<ProductDetailEntity> Products { get; set; } = null!;

    public ProductsWriteStore(DbContextOptions<ProductsWriteStore> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductDetailEntity>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.Name).IsRequired(false);
            b.Property(p => p.Description).IsRequired(false);
            b.ToTable("Products");
        });
    }

    public async Task UpdateProduct(int id, ProductDetails details)
    {
        // Avoid extra round-trip: attach a stub entity and mark only changed properties as modified
        var product = new ProductDetailEntity { Id = id };
        Attach(product);

        product.Name = details.Name;
        product.Description = details.Description;

        Entry(product).Property(p => p.Name).IsModified = true;
        Entry(product).Property(p => p.Description).IsModified = true;

        await SaveChangesAsync();
    }

    public async Task<int> CreateRecord(ProductDetails details)
    {
        ProductDetailEntity newProduct = new ProductDetailEntity()
        {
            Description = details.Description,
            Name = details.Name
        };
        
        Products.Add(newProduct);
        await SaveChangesAsync();
        
        return newProduct.Id;
    }
    
    
    private class ProductDetailEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}

