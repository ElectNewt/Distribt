using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;
using Distribt.Shared.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Distribt.Services.Products.BusinessLogic.Tests.DataAccess;

public class OutboxMessageServiceTests
{
    private readonly Mock<IOutboxRepository> _mockRepository;
    private readonly Mock<IDomainMessagePublisher> _mockPublisher;
    private readonly Mock<ISerializer> _mockSerializer;
    private readonly Mock<ILogger<OutboxMessageService>> _mockLogger;
    private readonly OutboxMessageService _service;

    public OutboxMessageServiceTests()
    {
        _mockRepository = new Mock<IOutboxRepository>();
        _mockPublisher = new Mock<IDomainMessagePublisher>();
        _mockSerializer = new Mock<ISerializer>();
        _mockLogger = new Mock<ILogger<OutboxMessageService>>();

        _service = new OutboxMessageService(
            _mockRepository.Object,
            _mockPublisher.Object,
            _mockSerializer.Object,
            _mockLogger.Object);
    }



    [Fact]
    public async Task ProcessPendingMessagesAsync_ShouldProcessAllMessages_WhenMessagesExist()
    {
        // Arrange
        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "ProductCreated",
            EventData = "{\"Id\":123}",
            RoutingKey = "internal"
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "ProductUpdated",
            EventData = "{\"ProductId\":456}",
            RoutingKey = "internal"
        };

        var messages = new[] { message1, message2 };

        _mockRepository.Setup(x => x.GetUnprocessedMessagesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var productCreated = new ProductCreated(123, new CreateProductRequest(
            new ProductDetails("Test", "Description"), 10, 99.99m));
        var productUpdated = new ProductUpdated(456, new ProductDetails("Updated", "Updated Description"));

        _mockSerializer.Setup(x => x.DeserializeObject<ProductCreated>(message1.EventData))
            .Returns(productCreated);
        _mockSerializer.Setup(x => x.DeserializeObject<ProductUpdated>(message2.EventData))
            .Returns(productUpdated);

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _mockPublisher.Verify(x => x.Publish(productCreated, null, "internal", It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.Publish(productUpdated, null, "internal", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.MarkAsProcessedAsync(message1.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.MarkAsProcessedAsync(message2.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_ShouldMarkAsFailedAndContinue_WhenPublishingFails()
    {
        // Arrange
        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "ProductCreated",
            EventData = "{\"Id\":123}",
            RoutingKey = "internal"
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "ProductUpdated",
            EventData = "{\"ProductId\":456}",
            RoutingKey = "internal"
        };

        var messages = new[] { message1, message2 };

        _mockRepository.Setup(x => x.GetUnprocessedMessagesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var productCreated = new ProductCreated(123, new CreateProductRequest(
            new ProductDetails("Test", "Description"), 10, 99.99m));
        var productUpdated = new ProductUpdated(456, new ProductDetails("Updated", "Updated Description"));

        _mockSerializer.Setup(x => x.DeserializeObject<ProductCreated>(message1.EventData))
            .Returns(productCreated);
        _mockSerializer.Setup(x => x.DeserializeObject<ProductUpdated>(message2.EventData))
            .Returns(productUpdated);

        // Setup first message to fail
        _mockPublisher.Setup(x => x.Publish(productCreated, null, "internal", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Publishing failed"));

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _mockRepository.Verify(x => x.MarkAsFailedAsync(message1.Id, "Publishing failed", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.MarkAsProcessedAsync(message2.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.Publish(productUpdated, null, "internal", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ProductCreated")]
    [InlineData("ProductUpdated")]
    public async Task ProcessPendingMessagesAsync_ShouldDeserializeCorrectEventType_WhenEventTypeIsSupported(string eventType)
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EventData = "{\"test\":\"data\"}",
            RoutingKey = "internal"
        };

        _mockRepository.Setup(x => x.GetUnprocessedMessagesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { message });

        var mockEvent = eventType == "ProductCreated" 
            ? (object)new ProductCreated(123, new CreateProductRequest(new ProductDetails("Test", "Desc"), 10, 99.99m))
            : new ProductUpdated(123, new ProductDetails("Test", "Desc"));

        if (eventType == "ProductCreated")
        {
            _mockSerializer.Setup(x => x.DeserializeObject<ProductCreated>(message.EventData))
                .Returns((ProductCreated)mockEvent);
        }
        else
        {
            _mockSerializer.Setup(x => x.DeserializeObject<ProductUpdated>(message.EventData))
                .Returns((ProductUpdated)mockEvent);
        }

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _mockPublisher.Verify(x => x.Publish(mockEvent, null, "internal", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_ShouldThrowException_WhenUnknownEventType()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "UnknownEvent",
            EventData = "{\"test\":\"data\"}",
            RoutingKey = "internal"
        };

        _mockRepository.Setup(x => x.GetUnprocessedMessagesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { message });

        // Act
        await _service.ProcessPendingMessagesAsync();

        // Assert
        _mockRepository.Verify(x => x.MarkAsFailedAsync(message.Id, 
            It.Is<string>(s => s.Contains("Unknown event type: UnknownEvent")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
} 