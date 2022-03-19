using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Distribt.Shared.Discovery;

namespace Distribt.Services.Products.BusinessLogic.UseCases;


public interface ICreateProductDetails
{
    Task<CreateProductResponse> Execute(CreateProductRequest productRequest);
}

public class CreateProductDetails : ICreateProductDetails
{
    private readonly IProductsWriteStore _writeStore;
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly IServiceDiscovery _discovery;
    private readonly IStockApi _stockApi;
    private readonly IWarehouseApi _warehouseApi;

    public CreateProductDetails(IProductsWriteStore writeStore, IDomainMessagePublisher domainMessagePublisher, IServiceDiscovery discovery, IStockApi stockApi, IWarehouseApi warehouseApi)
    {
        _writeStore = writeStore;
        _domainMessagePublisher = domainMessagePublisher;
        _discovery = discovery;
        _stockApi = stockApi;
        _warehouseApi = warehouseApi;
    }
    
    
    public async Task<CreateProductResponse> Execute(CreateProductRequest productRequest)
    {
       int productId = await _writeStore.CreateRecord(productRequest.Details);

       await _stockApi.AddStockToProduct(productId, productRequest.Stock);

       await _warehouseApi.ModifySalesPrice(productId, productRequest.Price);
        
        await _domainMessagePublisher.Publish(new ProductCreated(productId, productRequest), routingKey: "internal");
        
        string getUrl = await _discovery.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead);

        return new CreateProductResponse($"{getUrl}/product/{productId}");
    }
}

public record CreateProductResponse(string Url);





//The following two interfaces represent the two services that our product creation depends on.
//Note: we will see sagas in the future.
public interface IStockApi
{
    Task<bool> AddStockToProduct(int id, int stock)
    {
        return Task.FromResult(true);
    }
}

public interface IWarehouseApi
{
    Task<bool> ModifySalesPrice(int id, decimal price)
    {
        return Task.FromResult(true);
    }
    
}

public class ProductsDependencyFakeType : IWarehouseApi, IStockApi
{
    //This is a fake type for the DI
}