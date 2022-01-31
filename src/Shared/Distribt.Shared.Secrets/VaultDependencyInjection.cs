using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Secrets;

public static class VaultDependencyInjection
{
    public static void AddVaultService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<VaultSettings>(configuration.GetSection("SecretManager"));
        serviceCollection.AddScoped<ISecretManager, VaultSecretManager>();
    }
}