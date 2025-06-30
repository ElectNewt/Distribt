using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;


public interface IProductsWriteStore
{
    Task UpdateProduct(int id, ProductDetails details);
    Task<int> CreateRecord(ProductDetails details);
    Task UpdateProductWithOutbox(int id, ProductDetails details);
    Task<int> CreateRecordWithOutbox(CreateProductRequest request);
}

public class ProductsWriteStore : DbContext, IProductsWriteStore
{
    private DbSet<ProductDetailEntity> Products { get; set; } = null!;
    private DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

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

    public async Task<int> CreateRecordWithOutbox(CreateProductRequest request)
    {
        var useTransaction = Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
        var transaction = useTransaction ? await Database.BeginTransactionAsync() : null;
        ProductDetailEntity newProduct = new ProductDetailEntity()
        {
            Description = request.Details.Description,
            Name = request.Details.Name
        };
        var result = await Products.AddAsync(newProduct);
        await SaveChangesAsync();
        var eventMessage = new ProductCreated(result.Entity.Id ?? 0, request);
        OutboxMessages.Add(CreateOutboxMessage(eventMessage));
        await SaveChangesAsync();
        if (useTransaction && transaction != null)
            await transaction.CommitAsync();
        return result.Entity.Id ?? throw new ApplicationException("the record has not been inserted in the db");
    }

    public async Task UpdateProductWithOutbox(int id, ProductDetails details)
    {
        var useTransaction = Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
        var transaction = useTransaction ? await Database.BeginTransactionAsync() : null;
        var product = await Products.SingleAsync(a => a.Id == id);
        product.Description = details.Description;
        product.Name = details.Name;
        OutboxMessages.Add(CreateOutboxMessage(new ProductUpdated(id, details)));
        await SaveChangesAsync();
        if (useTransaction && transaction != null)
            await transaction.CommitAsync();
    }

    internal async Task<List<OutboxMessageEntity>> GetPendingOutboxMessages(CancellationToken cancellationToken)
        => await OutboxMessages.Where(a => a.SentUtc == null).ToListAsync(cancellationToken);

    internal async Task MarkAsSent(IEnumerable<OutboxMessageEntity> messages, CancellationToken cancellationToken)
    {
        foreach (var msg in messages)
        {
            msg.SentUtc = DateTime.UtcNow;
            _ = OutboxMessages.Update(msg);
        }
        await SaveChangesAsync(cancellationToken);
    }

    private static OutboxMessageEntity CreateOutboxMessage(object message)
        => new OutboxMessageEntity
        {
            Type = message.GetType().AssemblyQualifiedName!,
            Payload = System.Text.Json.JsonSerializer.Serialize(message),
            CreatedUtc = DateTime.UtcNow
        };

    private class ProductDetailEntity
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}

