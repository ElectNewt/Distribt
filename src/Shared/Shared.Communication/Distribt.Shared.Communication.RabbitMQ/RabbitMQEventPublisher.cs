using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher;
using Distribt.Shared.Serialization;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Distribt.Shared.Communication.RabbitMQ;

public class RabbitMQEventPublisher : IRabbitMQPublisher
{
    private readonly ISerializer _serializer;
    private readonly RabbitMQSettings _settings;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMQEventPublisher(ISerializer serializer, IOptions<RabbitMQSettings> settings)
    {
        _serializer = serializer;
        _settings = settings.Value;
        _connectionFactory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            Password = _settings.Credentials!.password,
            UserName = _settings.Credentials.username
        };
    }

    public Task Publish(string messageType, string content)
    {
        using IConnection connection = _connectionFactory.CreateConnection();
        using IModel model = connection.CreateModel();

        var properties = model.CreateBasicProperties();
        properties.Persistent = true;
        properties.Type = messageType;

        model.BasicPublish(exchange: GetCorrectExchange(messageType),
            routingKey: "", // Assuming routing key is not used for outbox messages, or can be derived from messageType
            basicProperties: properties,
            body: _serializer.SerializeObjectToByteArray(content));

        return Task.CompletedTask;
    }

    private string GetCorrectExchange(string messageType)
    {
        // This logic might need to be refined based on how messageType maps to exchanges
        // For simplicity, assuming all outbox messages go to a domain exchange
        return _settings.Publisher?.DomainExchange ?? throw new ArgumentException("please configure the DomainExchange on the appsettings");
    }
}
