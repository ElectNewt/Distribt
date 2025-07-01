namespace Distribt.Shared.Communication.RabbitMQ;

public interface IRabbitMQPublisher
{
    Task Publish(string messageType, string content);
}