using System;
using System.Linq;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic;

public class OutboxTests
{
    private ProductsWriteStore CreateInMemoryStore()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductsWriteStore(options);
    }

    [Fact]
    public async Task AddOutboxMessage_ShouldStoreMessageCorrectly()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var productCreated = new ProductCreated(1, new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 10, 99.99m));
        
        await store.AddOutboxMessage(nameof(ProductCreated), productCreated, "internal");
        await store.SaveChangesAsync();

        var messages = await store.GetUnprocessedOutboxMessages();
        
        Assert.Single(messages);
        var message = messages.First();
        Assert.Equal(nameof(ProductCreated), message.EventType);
        Assert.Equal("internal", message.RoutingKey);
        Assert.False(message.IsProcessed);
        Assert.Null(message.ProcessedAt);
        
        var deserializedEvent = JsonSerializer.Deserialize<ProductCreated>(message.EventData);
        Assert.NotNull(deserializedEvent);
        Assert.Equal(1, deserializedEvent.Id);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessages_ShouldReturnOnlyUnprocessedMessages()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var productCreated1 = new ProductCreated(1, new CreateProductRequest(
            new ProductDetails("Product 1", "Description 1"), 10, 99.99m));
        var productCreated2 = new ProductCreated(2, new CreateProductRequest(
            new ProductDetails("Product 2", "Description 2"), 20, 199.99m));

        await store.AddOutboxMessage(nameof(ProductCreated), productCreated1, "internal");
        await store.AddOutboxMessage(nameof(ProductCreated), productCreated2, "internal");
        await store.SaveChangesAsync();

        var unprocessedMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Equal(2, unprocessedMessages.Count);

        await store.MarkOutboxMessageAsProcessed(unprocessedMessages.First().Id);

        var remainingUnprocessed = await store.GetUnprocessedOutboxMessages();
        Assert.Single(remainingUnprocessed);
    }

    [Fact]
    public async Task MarkOutboxMessageAsProcessed_ShouldUpdateMessageCorrectly()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var productUpdated = new ProductUpdated(1, new ProductDetails("Updated Product", "Updated Description"));
        
        await store.AddOutboxMessage(nameof(ProductUpdated), productUpdated, "internal");
        await store.SaveChangesAsync();

        var messages = await store.GetUnprocessedOutboxMessages();
        var messageId = messages.First().Id;

        await store.MarkOutboxMessageAsProcessed(messageId);

        var unprocessedMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Empty(unprocessedMessages);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessages_ShouldReturnMessagesOrderedByCreatedAt()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var productCreated1 = new ProductCreated(1, new CreateProductRequest(
            new ProductDetails("Product 1", "Description 1"), 10, 99.99m));
        var productCreated2 = new ProductCreated(2, new CreateProductRequest(
            new ProductDetails("Product 2", "Description 2"), 20, 199.99m));

        await store.AddOutboxMessage(nameof(ProductCreated), productCreated1, "internal");
        await Task.Delay(10); // Ensure different timestamps
        await store.AddOutboxMessage(nameof(ProductCreated), productCreated2, "internal");
        await store.SaveChangesAsync();

        var messages = await store.GetUnprocessedOutboxMessages();
        
        Assert.Equal(2, messages.Count);
        Assert.True(messages[0].CreatedAt <= messages[1].CreatedAt);
    }
}