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
    
    
    private class ProductDetailEntity
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

}

