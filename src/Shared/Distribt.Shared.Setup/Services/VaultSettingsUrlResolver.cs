using Microsoft.Extensions.Options;

namespace Distribt.Shared.Setup.Services;

internal class VaultSettingsUrlResolver : IPostConfigureOptions<VaultSettings>
{
    private readonly IServiceProvider _serviceProvider;

    public VaultSettingsUrlResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, VaultSettings options)
    {
        var serviceDiscovery = _serviceProvider.GetService<IServiceDiscovery>();
        if (serviceDiscovery == null) return;

        string url = serviceDiscovery.GetFullAddress(DiscoveryServices.Secrets)
            .GetAwaiter().GetResult();
        options.UpdateUrl(url);
    }
}