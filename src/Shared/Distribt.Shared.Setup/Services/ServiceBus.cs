using Distribt.Shared.Communication.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Setup.Services;

public static class ServiceBus
{
    public static void AddServiceBusIntegrationPublisher(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(GetRabbitMqSecretCredentialsfromRabbitMQEngine, GetRabbitMQHostName,
            configuration);
        serviceCollection.AddRabbitMQPublisher<IntegrationMessage>();
    }

    /// <summary>
    /// default option (KeyValue) to get credentials using Vault 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    private static async Task<RabbitMQCredentials> GetRabbitMqSecretCredentials(IServiceProvider serviceProvider)
    {
        var secretManager = serviceProvider.GetService<ISecretManager>();
        return await secretManager!.Get<RabbitMQCredentials>("rabbitmq");
    }

    /// <summary>
    /// this option is used to show the usage of different engines on Vault
    /// </summary>
    private static async Task<RabbitMQCredentials> GetRabbitMqSecretCredentialsfromRabbitMQEngine(
        IServiceProvider serviceProvider)
    {
        var secretManager = serviceProvider.GetService<ISecretManager>();
        var credentials = await secretManager!.GetRabbitMQCredentials("distribt-role");
        return new RabbitMQCredentials() { password = credentials.Password, username = credentials.Username };
    }

    public static void AddServiceBusIntegrationConsumer(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(GetRabbitMqSecretCredentials, GetRabbitMQHostName, configuration);
        serviceCollection.AddRabbitMqConsumer<IntegrationMessage>();
    }

    public static void AddServiceBusDomainPublisher(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(GetRabbitMqSecretCredentials, GetRabbitMQHostName, configuration);
        serviceCollection.AddRabbitMQPublisher<DomainMessage>();
    }

    public static void AddServiceBusDomainConsumer(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(GetRabbitMqSecretCredentials, GetRabbitMQHostName, configuration);
        serviceCollection.AddRabbitMqConsumer<DomainMessage>();
    }

    //#8 The handlers have to be somehow injected automatically because it will facilitate the usage with DI
    public static void AddHandlers(this IServiceCollection serviceCollection, IEnumerable<IMessageHandler> handlers)
    {
        serviceCollection.AddConsumerHandlers(handlers);
    }

    private static async Task<string> GetRabbitMQHostName(IServiceProvider serviceProvider)
    {
        var serviceDiscovery = serviceProvider.GetService<IServiceDiscovery>();
        return await serviceDiscovery?.GetFullAddress(DiscoveryServices.RabbitMQ)!;
    }
}