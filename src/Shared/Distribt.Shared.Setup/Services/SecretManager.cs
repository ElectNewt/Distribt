using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Distribt.Shared.Setup.Services;

public static class SecretManager
{
    public static void AddSecretManager(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddVaultService(configuration);
        serviceCollection.AddSingleton<IPostConfigureOptions<VaultSettings>, VaultSettingsUrlResolver>();
    }
}