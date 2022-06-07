using Distribt.Shared.Discovery;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Distribt.Services.Orders.BusinessLogic.HealthChecks;

public class ProductsHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceDiscovery _discovery;

    public ProductsHealthCheck(IHttpClientFactory httpClientFactory, IServiceDiscovery discovery)
    {
        _httpClientFactory = httpClientFactory;
        _discovery = discovery;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        //TODO: abstract out all the HTTP calls to other distribt microservices #26
        HttpClient client = _httpClientFactory.CreateClient();
        string productsReadApi =
            await _discovery.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead, cancellationToken);
        client.BaseAddress = new Uri(productsReadApi);
        HttpResponseMessage responseMessage = await client.GetAsync($"health", cancellationToken);
        if (responseMessage.IsSuccessStatusCode)
        {
            return HealthCheckResult.Healthy("Product service is healthy");
        }
        
        return HealthCheckResult.Degraded("Product service is down");
    }
}