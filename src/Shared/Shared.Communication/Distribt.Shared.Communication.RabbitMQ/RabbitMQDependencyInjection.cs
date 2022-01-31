using Distribt.Shared.Communication.Consumer;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher;
using Distribt.Shared.Communication.RabbitMQ.Consumer;
using Distribt.Shared.Communication.RabbitMQ.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Communication.RabbitMQ;

public static class RabbitMQDependencyInjection
{
    public static void AddRabbitMQ(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<RabbitMQCredentials>> rabbitMqCredentialsFactory, IConfiguration configuration)
    {
        serviceCollection.AddRabbitMQ(configuration);
        serviceCollection.PostConfigure<RabbitMQSettings>(x => x.Credentials
            = rabbitMqCredentialsFactory.Invoke(serviceCollection.BuildServiceProvider()).Result);
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