using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public class UpdateProductDetailsWithOutbox : IUpdateProductDetails
{
    private readonly IProductsWriteStore _writeStore;

    public UpdateProductDetailsWithOutbox(IProductsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<bool> Execute(int id, ProductDetails productDetails)
    {
        // Create the event that will be published via outbox
        var productUpdatedEvent = new ProductUpdated(id, productDetails);
        
        // Create outbox message
        var outboxMessage = new OutboxMessage
        {
            EventType = nameof(ProductUpdated),
            EventData = System.Text.Json.JsonSerializer.Serialize(productUpdatedEvent),
            RoutingKey = "internal"
        };
        
        // Update product and store outbox message in the same transaction  
        await _writeStore.UpdateProductWithOutbox(id, productDetails, outboxMessage);
        
        return true;
    }
} 