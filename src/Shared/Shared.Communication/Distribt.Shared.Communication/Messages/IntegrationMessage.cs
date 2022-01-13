namespace Distribt.Shared.Communication.Messages;

public record IntegrationMessage : IMessage
{
    public string MessageIdentifier { get; }
    public string Name { get; }
    public IntegrationMessage(string messageIdentifier, string name)
    {
        MessageIdentifier = messageIdentifier;
        Name = name;
    }
}

public record IntegrationMessage<T> : IntegrationMessage
{
    public T Content { get; }
    public Metadata Metadata { get; }

    public IntegrationMessage(string messageIdentifier, string name, T content, Metadata metadata)
        : base(messageIdentifier, name)
    {
        Content = content;
        Metadata = metadata;
    }
}