using Distribt.Shared.Communication.RabbitMQ;
using Distribt.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Distribt.Shared.Outbox.Processor
{
    public class OutboxProcessor<TDbContext> : BackgroundService where TDbContext : DbContext
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxProcessor<TDbContext>> _logger;
        private readonly IRabbitMQPublisher _publisher;

        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessor<TDbContext>> logger, IRabbitMQPublisher publisher)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                    var outboxMessages = await dbContext.Set<OutboxMessage>()
                        .Where(m => m.ProcessedOn == null)
                        .OrderBy(m => m.OccurredOn)
                        .Take(100) // Process in batches
                        .ToListAsync(stoppingToken);

                    foreach (var message in outboxMessages)
                    {
                        try
                        {
                            // Publish the message
                            await _publisher.Publish(message.MessageType, message.Content);

                            // Mark as processed
                            message.ProcessedOn = DateTime.UtcNow;
                            dbContext.Set<OutboxMessage>().Update(message);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                            message.Error = ex.Message;
                            dbContext.Set<OutboxMessage>().Update(message);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Poll every 5 seconds
            }
        }
    }
}
