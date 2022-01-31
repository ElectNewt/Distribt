using System.Text;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher;
using Distribt.Shared.Serialization;
using Microsoft.Extensions.Options; 
using RabbitMQ.Client;

namespace Distribt.Shared.Communication.RabbitMQ.Publisher;

public class RabbitMQMessagePublisher<TMessage> : IExternalMessagePublisher<TMessage>
    where TMessage : IMessage
{
    private readonly ISerializer _serializer;
    private readonly RabbitMQSettings _settings;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMQMessagePublisher(ISerializer serializer, IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
        _serializer = serializer;
        _connectionFactory = new ConnectionFactory()
        {
            HostName = _settings.Hostname,
            Password = _settings.Credentials!.password,
            UserName = _settings.Credentials.username
        };
    }

    public Task Publish(TMessage message, string? routingKey = null, CancellationToken cancellationToken = default)
    {
        using IConnection connection = _connectionFactory.CreateConnection();
        using IModel model = connection.CreateModel();

        PublishSingle(message, model, routingKey);

        return Task.CompletedTask;
    }

    public Task PublishMany(IEnumerable<TMessage> messages, string? routingKey = null, CancellationToken cancellationToken = default)
    {
        using IConnection connection = _connectionFactory.CreateConnection();
        using IModel model = connection.CreateModel();
        foreach (TMessage message in messages)
        {
            PublishSingle(message, model, routingKey);
        }

        return Task.CompletedTask;
    }


   
    private void PublishSingle(TMessage message, IModel model, string? routingKey)
    {
        var properties = model.CreateBasicProperties();
        properties.Persistent = true; 
        properties.Type = RemoveVersion(message.GetType());

        model.BasicPublish(exchange: GetCorrectExchange(),
            routingKey: routingKey ?? "",
            basicProperties: properties,
            body: _serializer.SerializeObjectToByteArray(message));
    }

    private string GetCorrectExchange()
    {
        return (typeof(TMessage) == typeof(IntegrationMessage) 
            ? _settings.Publisher?.IntegrationExchange 
            : _settings.Publisher?.DomainExchange) 
               ?? throw  new ArgumentException("please configure the Exchanges on the appsettings");
    }

    /// <summary>
    /// there is a limit of 255 characters on the type in RabbitMQ.
    /// in top of that the version will cause issues if it gets updated and the payload contains the old and so on.  
    /// </summary>
    private string RemoveVersion(Type type)
    {
        return RemoveVersionFromQualifiedName(type.AssemblyQualifiedName ?? "", 0);
    }

    private string RemoveVersionFromQualifiedName(string assemblyQualifiedName, int indexStart)
    {
        var stringBuilder = new StringBuilder();
        var indexOfGenericClose = assemblyQualifiedName.IndexOf("]]", indexStart + 1, StringComparison.Ordinal);
        var indexOfVersion = assemblyQualifiedName.IndexOf(", Version", indexStart + 1, StringComparison.Ordinal);

        if (indexOfVersion < 0)
            return assemblyQualifiedName;

        stringBuilder.Append(assemblyQualifiedName.Substring(indexStart, indexOfVersion - indexStart));

        if (indexOfGenericClose > 0)
            stringBuilder.Append(RemoveVersionFromQualifiedName(assemblyQualifiedName, indexOfGenericClose));

        return stringBuilder.ToString();
    }
}