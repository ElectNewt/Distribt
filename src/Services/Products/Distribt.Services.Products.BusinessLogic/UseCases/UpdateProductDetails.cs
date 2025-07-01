using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public interface IUpdateProductDetails
{
    Task<bool> Execute(int id, ProductDetails productDetails);
}

public class UpdateProductDetails : IUpdateProductDetails
{
    private readonly IProductsWriteStore _writeStore;

    public UpdateProductDetails(IProductsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<bool> Execute(int id, ProductDetails productDetails)
    {
        await _writeStore.ExecuteInTransaction(async () =>
        {
            await _writeStore.UpdateProduct(id, productDetails);
            
            // Save the event to outbox within the same transaction
            var productUpdatedEvent = new ProductUpdated(id, productDetails);
            await _writeStore.SaveOutboxMessage(productUpdatedEvent, nameof(ProductUpdated));
        });
        
        return true;
    }
}