using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Setup;

public static class ServiceBus
{
    public static void AddServiceBusIntegrationPublisher(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.AddRabbitMQPublisher<IntegrationMessage>();
    }

    public static void AddServiceBusIntegrationConsumer(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.AddRabbitMqConsumer<IntegrationMessage>();
    }
    
    public static void AddServiceBusDomainPublisher(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.AddRabbitMQPublisher<DomainMessage>();
    }

    public static void AddServiceBusDomainConsumer(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.AddRabbitMqConsumer<DomainMessage>();
    }
    
    //#8 The handlers have to be somehow injected automatically because it will facilitate the usage with DI
    public static void AddHandlers(this IServiceCollection serviceCollection, IEnumerable<IMessageHandler> handlers)
    {
        serviceCollection.AddConsumerHandlers(handlers);
    }
}