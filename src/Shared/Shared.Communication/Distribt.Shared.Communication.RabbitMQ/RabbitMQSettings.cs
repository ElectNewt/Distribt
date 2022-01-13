namespace Distribt.Shared.Communication.RabbitMQ;

public record RabbitMQSettings
{
    public string Hostname { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public PublisherSettings? Publisher { get; init; }
    public ConsumerSettings? Consumer { get; init; }
}

public record PublisherSettings
{
    public string? IntegrationExchange { get; init; }
    public string? DomainExchange { get; init; }
}

public record ConsumerSettings
{
    public string? IntegrationExchanges { get; init; }
    public string? IntegrationQueue { get; init; }
    public string? DomainExchanges { get; init; }
    public string? DomainQueue { get; init; }
}