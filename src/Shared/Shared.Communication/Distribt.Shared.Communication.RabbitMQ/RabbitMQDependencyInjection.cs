using Distribt.Shared.Communication.Consumer;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher;
using Distribt.Shared.Communication.RabbitMQ.Consumer;
using Distribt.Shared.Communication.RabbitMQ.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Distribt.Shared.Communication.RabbitMQ;

public static class RabbitMQDependencyInjection
{
    public static void AddRabbitMQ(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<RabbitMQCredentials>> rabbitMqCredentialsFactory,
        Func<IServiceProvider, Task<string>> rabbitMqHostName,
        IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.PostConfigure<RabbitMQSettings>(x =>
        {
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            x.SetCredentials(rabbitMqCredentialsFactory.Invoke(serviceProvider).Result);
            x.SetHostName(rabbitMqHostName.Invoke(serviceProvider).Result);

            serviceCollection.AddRabbitMqHealthCheck();
        });
    }

    private static IServiceCollection AddRabbitMqHealthCheck(this IServiceCollection serviceCollection)
    {
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        RabbitMQSettings settings = serviceProvider.GetRequiredService<RabbitMQSettings>();
        serviceCollection.AddHealthChecks().AddRabbitMQ(settings.GetConnectionString(), HealthStatus.Unhealthy);
        return serviceCollection;
    }
    
    /// <summary>
    /// this method is used when the credentials are inside the configuration. not recommended.
    /// </summary>
    public static void AddRabbitMQ(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<RabbitMQSettings>(configuration.GetSection("Bus:RabbitMQ"));
    }
    
    public static void AddConsumerHandlers(this IServiceCollection serviceCollection, IEnumerable<IMessageHandler> handlers)
    {
        
        serviceCollection.AddSingleton<IMessageHandlerRegistry>(new MessageHandlerRegistry(handlers));
        serviceCollection.AddSingleton<IHandleMessage, HandleMessage>();
    }

    public static void AddRabbitMqConsumer<TMessage>(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddConsumer<TMessage>();
        serviceCollection.AddSingleton<IMessageConsumer<TMessage>, RabbitMQMessageConsumer<TMessage>>();
    }
    
    public static void AddRabbitMQPublisher<TMessage>(this IServiceCollection serviceCollection)
        where TMessage : IMessage
    {
        serviceCollection.AddPublisher<TMessage>();
        serviceCollection.AddSingleton<IExternalMessagePublisher<TMessage>, RabbitMQMessagePublisher<TMessage>>();
    }
    
    
}