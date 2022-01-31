using Distribt.Shared.Communication.Consumer;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ISerializer = Distribt.Shared.Serialization.ISerializer;

namespace Distribt.Shared.Communication.RabbitMQ.Consumer;

public class RabbitMQMessageConsumer<TMessage> : IMessageConsumer<TMessage>
{
    private readonly ISerializer _serializer;
    private readonly RabbitMQSettings _settings;
    private readonly ConnectionFactory _connectionFactory;
    private readonly IHandleMessage _handleMessage;


    public RabbitMQMessageConsumer(ISerializer serializer, IOptions<RabbitMQSettings> settings, IHandleMessage handleMessage)
    {
        _settings = settings.Value;
        _serializer = serializer;
        _handleMessage = handleMessage;
        _connectionFactory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            Password = _settings.Credentials!.password,
            UserName = _settings.Credentials.username
        };
    }

    public Task StartAsync(CancellationToken cancelToken = default)
    {
        return Task.Run(async () => await Consume(), cancelToken);
    }

    private Task Consume()
    {
        //I had to remove the usings in the next two statements
        //because the basicACk on the handler was giving "already disposed"
        IConnection connection = _connectionFactory.CreateConnection(); // #6 using (implement it correctly)
        IModel channel = connection.CreateModel(); // #6 using (implement it correctly)
        RabbitMQMessageReceiver receiver = new RabbitMQMessageReceiver(channel, _serializer, _handleMessage);
        string queue = GetCorrectQueue();
        
        channel.BasicConsume(queue, false, receiver);
        
       // #5 this should be here await consumer.HandleMessage();
       return Task.CompletedTask;
    }

    private string GetCorrectQueue()
    {
        return (typeof(TMessage) == typeof(IntegrationMessage)
                   ? _settings.Consumer?.IntegrationQueue
                   : _settings.Consumer?.DomainQueue)
               ?? throw new ArgumentException("please configure the queues on the appsettings");
    }
}