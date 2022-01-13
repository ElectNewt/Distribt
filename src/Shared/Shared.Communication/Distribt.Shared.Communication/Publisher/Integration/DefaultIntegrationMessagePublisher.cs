using Distribt.Shared.Communication.Messages;

namespace Distribt.Shared.Communication.Publisher.Integration;

public interface IIntegrationMessagePublisher
{
    Task Publish(object message, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default);
    Task PublishMany(IEnumerable<object> messages, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default);
}

public class DefaultIntegrationMessagePublisher : IIntegrationMessagePublisher
{
    private readonly IExternalMessagePublisher<IntegrationMessage> _externalPublisher;

    public DefaultIntegrationMessagePublisher(IExternalMessagePublisher<IntegrationMessage> externalPublisher)
    {
        _externalPublisher = externalPublisher;
    }

    public Task Publish(object message, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default)
    {
        Metadata calculatedMetadata = CalculateMetadata(metadata);
        var integrationMessage = IntegrationMessageMapper.MapToMessage(message, calculatedMetadata);
        return _externalPublisher.Publish(integrationMessage, routingKey, cancellationToken);
    }

    public Task PublishMany(IEnumerable<object> messages, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default)
    {
        var integrationMessages =
            messages.Select(a => IntegrationMessageMapper.MapToMessage(a, CalculateMetadata(metadata)));
        return _externalPublisher.PublishMany(integrationMessages, routingKey, cancellationToken);
    }

    private Metadata CalculateMetadata(Metadata? metadata)
    {
        return metadata ?? new Metadata(Guid.NewGuid().ToString(), DateTime.UtcNow);
    }
}