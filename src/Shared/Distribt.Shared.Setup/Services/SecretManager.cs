using Microsoft.Extensions.Configuration;

namespace Distribt.Shared.Setup.Services;

public static class SecretManager
{
    public static async Task AddSecretManager(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        //TODO: create an awaiter project instead of .result everywhere in the config
        string discoveredUrl = await GetVaultUrl(serviceCollection.BuildServiceProvider());
        serviceCollection.AddVaultService(configuration, discoveredUrl); 
    }

    private static async Task<string> GetVaultUrl(IServiceProvider serviceProvider)
    {
        var serviceDiscovery = serviceProvider.GetService<IServiceDiscovery>();
        return await serviceDiscovery?.GetFullAddress(DiscoveryServices.Secrets)!;
    }
}