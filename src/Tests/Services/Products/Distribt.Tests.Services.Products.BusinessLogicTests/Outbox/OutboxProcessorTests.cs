using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.Outbox;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public async Task CreateRecordWithOutboxMessage_ShouldCreateProductAndOutboxMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var productDetails = new ProductDetails("Test Product", "Test Description");
        var productRequest = new CreateProductRequest(productDetails, 10, 100m);

        // Act
        var productId = await dbContext.CreateRecordWithOutboxMessage(
            productDetails,
            typeof(ProductCreated).AssemblyQualifiedName!,
            new ProductCreated(0, productRequest), // Will be updated with actual ID
            "internal");

        // Assert
        Assert.True(productId > 0);

        var product = await dbContext.Set<ProductDetailEntity>().FirstAsync();
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);

        var outboxMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.Equal(typeof(ProductCreated).AssemblyQualifiedName, outboxMessage.EventType);
        Assert.Equal("internal", outboxMessage.RoutingKey);
        Assert.False(outboxMessage.IsProcessed);

        var deserializedEvent = JsonSerializer.Deserialize<ProductCreated>(outboxMessage.EventData);
        Assert.Equal(productId, deserializedEvent!.Id);
    }

    [Fact]
    public async Task OutboxProcessor_ShouldProcessUnprocessedMessages()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var mockPublisher = new Mock<IDomainMessagePublisher>();
        var mockLogger = new Mock<ILogger<OutboxProcessor>>();

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

        var processor = new OutboxProcessor(dbContext, mockPublisher.Object, mockLogger.Object);

        // Act
        await processor.ProcessPendingMessages();

        // Assert
        mockPublisher.Verify(p => p.Publish(It.IsAny<ProductCreated>(), null, "internal", default), Times.Once);

        var processedMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.True(processedMessage.IsProcessed);
        Assert.NotNull(processedMessage.ProcessedAt);
    }

    [Fact]
    public async Task GetUnprocessedMessages_ShouldReturnOnlyUnprocessedMessages()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

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
        var result = await dbContext.GetUnprocessedMessages();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsProcessed);
    }

    [Fact]
    public async Task MarkAsProcessed_ShouldUpdateMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

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
        await dbContext.MarkAsProcessed(message.Id);

        // Assert
        var updatedMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.True(updatedMessage.IsProcessed);
        Assert.NotNull(updatedMessage.ProcessedAt);
    }
}