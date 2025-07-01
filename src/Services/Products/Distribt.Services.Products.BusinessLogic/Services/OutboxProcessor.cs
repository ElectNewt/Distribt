using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Distribt.Services.Products.BusinessLogic.Services;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages()
    {
        using var scope = _serviceProvider.CreateScope();
        var writeStore = scope.ServiceProvider.GetRequiredService<IProductsWriteStore>();
        var domainMessagePublisher = scope.ServiceProvider.GetRequiredService<IDomainMessagePublisher>();

        var unprocessedMessages = await writeStore.GetUnprocessedOutboxMessages();
        
        foreach (var message in unprocessedMessages)
        {
            try
            {
                await PublishMessage(message, domainMessagePublisher);
                await writeStore.MarkOutboxMessageAsProcessed(message.Id);
                
                _logger.LogInformation("Successfully processed outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);
            }
        }
    }

    private async Task PublishMessage(OutboxMessage message, IDomainMessagePublisher publisher)
    {
        object eventData = message.EventType switch
        {
            nameof(ProductCreated) => JsonSerializer.Deserialize<ProductCreated>(message.EventData)!,
            nameof(ProductUpdated) => JsonSerializer.Deserialize<ProductUpdated>(message.EventData)!,
            _ => throw new ArgumentException($"Unknown event type: {message.EventType}")
        };

        await publisher.Publish(eventData, routingKey: message.RoutingKey);
    }
}