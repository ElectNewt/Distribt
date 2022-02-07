using System.Text;
using Consul;
using Microsoft.Extensions.Caching.Memory;

namespace Distribt.Shared.Discovery;

public interface IServiceDiscovery
{
    Task<string> GetFullAddress(string serviceKey, CancellationToken cancellationToken = default);
}

public class ConsulServiceDiscovery : IServiceDiscovery
{
    private readonly IConsulClient _client;
    private readonly MemoryCache _cache;

    public ConsulServiceDiscovery(IConsulClient client)
    {
        _client = client;
        _cache = new MemoryCache(new MemoryCacheOptions());
    }


    public async Task<string> GetFullAddress(string serviceKey, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(serviceKey, out string serviceAddress))
        {
            return serviceAddress;
        }

        return await GetAddressFromService(serviceKey, cancellationToken);
    }


    private async Task<string> GetAddressFromService(string serviceKey, CancellationToken cancellationToken = default)
    {
        
        var services = await _client.Catalog.Service(serviceKey, cancellationToken);
        if (services.Response != null && services.Response.Any())
        {
            var service = services.Response.First();
            StringBuilder serviceAddress = new StringBuilder();
            serviceAddress.Append(service.ServiceAddress);
            if (service.ServicePort != 0)
            {
                serviceAddress.Append($":{service.ServicePort}");
            }

            string serviceAddressString = serviceAddress.ToString();
            
            AddToCache(serviceKey, serviceAddressString);
            return serviceAddressString;
        }

        throw new ArgumentException($"seems like the service your are trying to access ({serviceKey}) does not exist ");
    }

    private void AddToCache(string serviceKey, string serviceAddress)
    {
        _cache.Set(serviceKey, serviceAddress);
    }
}