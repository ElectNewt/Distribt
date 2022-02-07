using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Setup.Services;

public static class ServiceDiscovery
{
    public static void AddServiceDiscovery(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddDiscovery(configuration); 
    }
}