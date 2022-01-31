using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Setup;

public static class SecretManager
{
    /// <summary>
    /// this is not in use because i consider that The vault can be specified on the DefaultDistribtWebApplication 
    /// </summary>
    public static void AddSecretManager(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddVaultService(configuration); 
    }
}