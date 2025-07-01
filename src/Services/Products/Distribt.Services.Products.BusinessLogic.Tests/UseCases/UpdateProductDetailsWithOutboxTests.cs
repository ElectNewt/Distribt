using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Serialization;
using Moq;
using Xunit;

namespace Distribt.Services.Products.BusinessLogic.Tests.UseCases;

public class UpdateProductDetailsWithOutboxTests
{
    private readonly Mock<IProductsWriteStore> _mockWriteStore;
    private readonly UpdateProductDetailsWithOutbox _useCase;

    public UpdateProductDetailsWithOutboxTests()
    {
        _mockWriteStore = new Mock<IProductsWriteStore>();

        _useCase = new UpdateProductDetailsWithOutbox(_mockWriteStore.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdateProductAndStoreOutboxMessage_WhenValidRequest()
    {
        // Arrange
        var productId = 123;
        var productDetails = new ProductDetails("Updated Product", "Updated Description");

        _mockWriteStore.Setup(x => x.UpdateProductWithOutbox(
                productId,
                productDetails,
                It.IsAny<OutboxMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Execute(productId, productDetails);

        // Assert
        Assert.True(result);
        
        _mockWriteStore.Verify(x => x.UpdateProductWithOutbox(
            productId,
            productDetails,
            It.Is<OutboxMessage>(om => 
                om.EventType == "ProductUpdated" &&
                om.EventData.Contains("\"ProductId\":123") &&
                om.RoutingKey == "internal")), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenWriteStoreFails()
    {
        // Arrange
        var productId = 123;
        var productDetails = new ProductDetails("Updated Product", "Updated Description");

        _mockWriteStore.Setup(x => x.UpdateProductWithOutbox(
                It.IsAny<int>(),
                It.IsAny<ProductDetails>(),
                It.IsAny<OutboxMessage>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(productId, productDetails));
    }

    [Fact]
    public async Task Execute_ShouldCreateCorrectOutboxMessage_WhenCalled()
    {
        // Arrange
        var productId = 123;
        var productDetails = new ProductDetails("Updated Product", "Updated Description");
        OutboxMessage? capturedOutboxMessage = null;

        _mockWriteStore.Setup(x => x.UpdateProductWithOutbox(
                It.IsAny<int>(),
                It.IsAny<ProductDetails>(),
                It.IsAny<OutboxMessage>()))
            .Callback<int, ProductDetails, OutboxMessage>((_, _, outboxMessage) =>
            {
                capturedOutboxMessage = outboxMessage;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.Execute(productId, productDetails);

        // Assert
        Assert.NotNull(capturedOutboxMessage);
        Assert.Equal("ProductUpdated", capturedOutboxMessage.EventType);
        Assert.Contains("\"ProductId\":123", capturedOutboxMessage.EventData);
        Assert.Equal("internal", capturedOutboxMessage.RoutingKey);
        Assert.False(capturedOutboxMessage.IsProcessed);
        Assert.Equal(0, capturedOutboxMessage.RetryCount);
    }
} 