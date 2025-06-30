using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.Outbox;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic.Outbox;

public class OutboxProcessorTests
{
    private ProductsWriteStore CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ProductsWriteStore(options);
    }

    [Fact]
    public async Task CreateRecord_ShouldCreateProduct()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var productDetails = new ProductDetails("Test Product", "Test Description");

        // Act
        var productId = await dbContext.CreateRecord(productDetails);

        // Assert
        Assert.True(productId > 0);

        var product = await dbContext.Set<ProductDetailEntity>().FirstAsync();
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);
    }

    [Fact]
    public async Task AddOutboxMessage_ShouldCreateOutboxMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var productCreated = new ProductCreated(1, new CreateProductRequest(new ProductDetails("Test", "Desc"), 10, 100m));

        // Act
        await dbContext.AddOutboxMessage(
            typeof(ProductCreated).AssemblyQualifiedName!,
            productCreated,
            "internal");

        // Assert
        var outboxMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.Equal(typeof(ProductCreated).AssemblyQualifiedName, outboxMessage.EventType);
        Assert.Equal("internal", outboxMessage.RoutingKey);
        Assert.False(outboxMessage.IsProcessed);

        var deserializedEvent = JsonSerializer.Deserialize<ProductCreated>(outboxMessage.EventData);
        Assert.Equal(1, deserializedEvent!.Id);
    }

    [Fact]
    public async Task OutboxProcessor_ShouldProcessUnprocessedMessages()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var mockPublisher = new Mock<IDomainMessagePublisher>();
        var mockLogger = new Mock<ILogger<OutboxProcessor>>();
        var outboxRepository = new OutboxRepository(dbContext);

        var productCreated = new ProductCreated(1, new CreateProductRequest(new ProductDetails("Test", "Desc"), 10, 100m));
        var outboxMessage = new OutboxMessage
        {
            EventType = typeof(ProductCreated).AssemblyQualifiedName!,
            EventData = JsonSerializer.Serialize(productCreated),
            RoutingKey = "internal",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false,
            RetryCount = 0
        };

        await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();

        var processor = new OutboxProcessor(outboxRepository, mockPublisher.Object, mockLogger.Object);

        // Act
        await processor.ProcessPendingMessages();

        // Assert
        mockPublisher.Verify(p => p.Publish(It.IsAny<ProductCreated>(), null, "internal", default), Times.Once);

        var processedMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.True(processedMessage.IsProcessed);
        Assert.NotNull(processedMessage.ProcessedAt);
    }

    [Fact]
    public async Task OutboxRepository_GetUnprocessedMessages_ShouldReturnOnlyUnprocessedMessages()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var repository = new OutboxRepository(dbContext);

        var processedMessage = new OutboxMessage
        {
            EventType = "Test",
            EventData = "{}",
            RoutingKey = "test",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = true,
            RetryCount = 0
        };

        var unprocessedMessage = new OutboxMessage
        {
            EventType = "Test",
            EventData = "{}",
            RoutingKey = "test",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false,
            RetryCount = 0
        };

        await dbContext.Set<OutboxMessage>().AddRangeAsync(processedMessage, unprocessedMessage);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetUnprocessedMessages();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsProcessed);
    }

    [Fact]
    public async Task OutboxRepository_MarkAsProcessed_ShouldUpdateMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var repository = new OutboxRepository(dbContext);

        var message = new OutboxMessage
        {
            EventType = "Test",
            EventData = "{}",
            RoutingKey = "test",
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false,
            RetryCount = 0
        };

        await dbContext.Set<OutboxMessage>().AddAsync(message);
        await dbContext.SaveChangesAsync();

        // Act
        await repository.MarkAsProcessed(message.Id);

        // Assert
        var updatedMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.True(updatedMessage.IsProcessed);
        Assert.NotNull(updatedMessage.ProcessedAt);
    }
}