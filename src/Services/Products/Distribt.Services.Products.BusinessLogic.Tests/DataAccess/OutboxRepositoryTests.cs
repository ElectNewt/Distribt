using Distribt.Services.Products.BusinessLogic.DataAccess;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Distribt.Services.Products.BusinessLogic.Tests.DataAccess;

public class OutboxRepositoryTests : IDisposable
{
    private readonly ProductsWriteStore _context;
    private readonly OutboxRepository _repository;

    public OutboxRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductsWriteStore(options);
        _repository = new OutboxRepository(_context);
    }

    [Fact]
    public async Task AddOutboxMessageAsync_ShouldAddMessage_WhenValidMessage()
    {
        // Arrange
        var message = new OutboxMessage
        {
            EventType = "TestEvent",
            EventData = "{\"test\": \"data\"}",
            RoutingKey = "test.routing"
        };

        // Act
        await _repository.AddOutboxMessageAsync(message);
        await _context.SaveChangesAsync();

        // Assert
        var savedMessage = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        Assert.NotNull(savedMessage);
        Assert.Equal("TestEvent", savedMessage.EventType);
        Assert.Equal("{\"test\": \"data\"}", savedMessage.EventData);
        Assert.Equal("test.routing", savedMessage.RoutingKey);
        Assert.False(savedMessage.IsProcessed);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ShouldReturnUnprocessedMessages_WhenMessagesExist()
    {
        // Arrange
        var processedMessage = new OutboxMessage
        {
            EventType = "ProcessedEvent",
            EventData = "{}",
            IsProcessed = true
        };

        var unprocessedMessage1 = new OutboxMessage
        {
            EventType = "UnprocessedEvent1",
            EventData = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var unprocessedMessage2 = new OutboxMessage
        {
            EventType = "UnprocessedEvent2",
            EventData = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };

        var failedMessage = new OutboxMessage
        {
            EventType = "FailedEvent",
            EventData = "{}",
            RetryCount = 3 // Exceeds retry limit
        };

        await _context.OutboxMessages.AddRangeAsync(processedMessage, unprocessedMessage1, unprocessedMessage2, failedMessage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUnprocessedMessagesAsync(10);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, m => m.EventType == "UnprocessedEvent1");
        Assert.Contains(result, m => m.EventType == "UnprocessedEvent2");
        
        // Should be ordered by CreatedAt (oldest first)
        var orderedResult = result.ToList();
        Assert.Equal("UnprocessedEvent1", orderedResult[0].EventType);
        Assert.Equal("UnprocessedEvent2", orderedResult[1].EventType);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldUpdateMessage_WhenMessageExists()
    {
        // Arrange
        var message = new OutboxMessage
        {
            EventType = "TestEvent",
            EventData = "{}",
            RetryCount = 1,
            ErrorMessage = "Previous error"
        };

        await _context.OutboxMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkAsProcessedAsync(message.Id);

        // Assert
        var updatedMessage = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        Assert.NotNull(updatedMessage);
        Assert.True(updatedMessage.IsProcessed);
        Assert.NotNull(updatedMessage.ProcessedAt);
        Assert.Null(updatedMessage.ErrorMessage);
    }

    [Fact]
    public async Task MarkAsFailedAsync_ShouldIncrementRetryCountAndSetError_WhenMessageExists()
    {
        // Arrange
        var message = new OutboxMessage
        {
            EventType = "TestEvent",
            EventData = "{}",
            RetryCount = 1
        };

        await _context.OutboxMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        var errorMessage = "Processing failed";

        // Act
        await _repository.MarkAsFailedAsync(message.Id, errorMessage);

        // Assert
        var updatedMessage = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        Assert.NotNull(updatedMessage);
        Assert.Equal(2, updatedMessage.RetryCount);
        Assert.Equal(errorMessage, updatedMessage.ErrorMessage);
        Assert.False(updatedMessage.IsProcessed);
    }

    [Fact]
    public async Task GetUnprocessedMessageCountAsync_ShouldReturnCorrectCount_WhenMessagesExist()
    {
        // Arrange
        var processedMessage = new OutboxMessage { EventType = "Processed", EventData = "{}", IsProcessed = true };
        var unprocessedMessage1 = new OutboxMessage { EventType = "Unprocessed1", EventData = "{}" };
        var unprocessedMessage2 = new OutboxMessage { EventType = "Unprocessed2", EventData = "{}" };
        var failedMessage = new OutboxMessage { EventType = "Failed", EventData = "{}", RetryCount = 3 };

        await _context.OutboxMessages.AddRangeAsync(processedMessage, unprocessedMessage1, unprocessedMessage2, failedMessage);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetUnprocessedMessageCountAsync();

        // Assert
        Assert.Equal(2, count); // Only unprocessed messages with retry count < 3
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 