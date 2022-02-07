using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Secrets;

public static class VaultDependencyInjection
{
    /// <summary>
    ///  add vault service to the service collection
    /// discovered url is optional as it will be calculated on the setup project, but only if that project is in use.
    /// </summary>
    public static void AddVaultService(this IServiceCollection serviceCollection, IConfiguration configuration, string? discoveredUrl = null)
    {
        serviceCollection.Configure<VaultSettings>(configuration.GetSection("SecretManager"));
        serviceCollection.PostConfigure<VaultSettings>(settings =>
        {
            if(!string.IsNullOrWhiteSpace(discoveredUrl))
                settings.UpdateUrl(discoveredUrl);
        });
        serviceCollection.AddScoped<ISecretManager, VaultSecretManager>();
    }
}