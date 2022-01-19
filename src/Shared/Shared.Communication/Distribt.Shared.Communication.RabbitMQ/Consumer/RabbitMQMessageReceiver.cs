using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Serialization;
using RabbitMQ.Client;

namespace Distribt.Shared.Communication.RabbitMQ.Consumer;

public class RabbitMQMessageReceiver : DefaultBasicConsumer
{
    private readonly IModel _channel;
    private readonly ISerializer _serializer;
    private byte[]? MessageBody { get; set; }
    private Type? MessageType { get; set; }
    private ulong DeliveryTag { get; set; }
    private readonly IHandleMessage _handleMessage;

    public RabbitMQMessageReceiver(IModel channel, ISerializer serializer, IHandleMessage handleMessage)
    {
        _channel = channel;
        _serializer = serializer;
        _handleMessage = handleMessage;
    }

    public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange,
        string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
    {
        MessageType = Type.GetType(properties.Type)!;
        MessageBody = body.ToArray();
        DeliveryTag = deliveryTag; // Used to delete the message from rabbitMQ

        // #5 not ideal solution, but seems that this HandleBasicDeliver needs to be like this as its not async
        var t = Task.Run(HandleMessage);
        t.Wait();
    }

    private async Task HandleMessage()
    {
        if (MessageBody == null || MessageType == null)
        {
            throw new ArgumentException("Neither the body or the messageType has been populated");
        }

        IMessage message = (_serializer.DeserializeObject(MessageBody, MessageType) as IMessage)
                           ?? throw new ArgumentException("The message did not deserialized properly");
        
        await _handleMessage.Handle(message, CancellationToken.None);
     
        _channel.BasicAck(DeliveryTag, false);
    }
}