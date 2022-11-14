using System.Net.Http.Json;
using Distribt.Services.Orders.BusinessLogic.Data.External;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;
using Microsoft.Extensions.Caching.Distributed;

namespace Distribt.Services.Orders.BusinessLogic.Services.External;

public interface IProductNameService
{
    Task<string> GetProductName(int id, CancellationToken cancellationToken = default(CancellationToken));
    Task SetProductName(int id, string name, CancellationToken cancellationToken = default(CancellationToken));
}

//TODO: #25
public class ProductNameService : IProductNameService
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceDiscovery _discovery;

    public ProductNameService(IProductRepository productRepository, IDistributedCache cache,
        IHttpClientFactory httpClientFactory, IServiceDiscovery discovery)
    {
        _productRepository = productRepository;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _discovery = discovery;
    }


    public async Task<string> GetProductName(int id, CancellationToken cancellationToken = default(CancellationToken))
    {
        string? value = await _cache.GetStringAsync($"ORDERS-PRODUCT::{id}", cancellationToken);
        if (value!=null)
        {
            return value;
        }
        string productName = await RetrieveProductName(id, cancellationToken);

        return productName;
    }

    public async Task SetProductName(int id, string name,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        await _productRepository.UpsertProductName(id, name, cancellationToken);
        await _cache.RemoveAsync($"ORDERS-PRODUCT::{id}", cancellationToken);
        await _cache.SetStringAsync($"ORDERS-PRODUCT::{id}", name, cancellationToken);
    }
   
    
    private async Task<string> RetrieveProductName(int id, CancellationToken cancellationToken)
    {
        string? productName = await _productRepository.GetProductName(id, cancellationToken);

        if (productName == null)
        {
            FullProductResponse product = await GetProduct(id, cancellationToken);
            await SetProductName(id, product.Details.Name, cancellationToken);
            productName = product.Details.Name;
        }

        return productName;
    }
    
    private async Task<FullProductResponse> GetProduct(int productId, CancellationToken cancellationToken = default(CancellationToken))
    {
        //TODO: abstract out all the HTTP calls to other distribt microservices #26
        HttpClient client = _httpClientFactory.CreateClient();
        string productsReadApi =
            await _discovery.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead, cancellationToken);
        client.BaseAddress = new Uri(productsReadApi);

        //TODO: replace exception
        return await client.GetFromJsonAsync<FullProductResponse>($"product/{productId}", cancellationToken) ?? 
               throw  new ArgumentException("Product does not exist");
    }
}