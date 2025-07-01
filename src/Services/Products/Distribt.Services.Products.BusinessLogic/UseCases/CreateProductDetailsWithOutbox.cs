using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public class CreateProductDetailsWithOutbox : ICreateProductDetails
{
    private readonly IProductsWriteStore _writeStore;
    private readonly IServiceDiscovery _discovery;
    private readonly IStockApi _stockApi;
    private readonly IWarehouseApi _warehouseApi;

    public CreateProductDetailsWithOutbox(
        IProductsWriteStore writeStore, 
        IServiceDiscovery discovery, 
        IStockApi stockApi, 
        IWarehouseApi warehouseApi)
    {
        _writeStore = writeStore;
        _discovery = discovery;
        _stockApi = stockApi;
        _warehouseApi = warehouseApi;
    }
    
    public async Task<CreateProductResponse> Execute(CreateProductRequest productRequest)
    {
        // Create product and store outbox message in the same transaction
        var (productId, _) = await _writeStore.CreateProductWithOutboxCallback(
            productRequest.Details,
            (id) => 
            {
                var productCreatedEvent = new ProductCreated(id, productRequest);
                return new OutboxMessage
                {
                    EventType = nameof(ProductCreated),
                    EventData = System.Text.Json.JsonSerializer.Serialize(productCreatedEvent),
                    RoutingKey = "internal"
                };
            }
        );

        await _stockApi.AddStockToProduct(productId, productRequest.Stock);
        await _warehouseApi.ModifySalesPrice(productId, productRequest.Price);
        
        string getUrl = await _discovery.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead);

        return new CreateProductResponse($"{getUrl}/product/{productId}");
    }
} 