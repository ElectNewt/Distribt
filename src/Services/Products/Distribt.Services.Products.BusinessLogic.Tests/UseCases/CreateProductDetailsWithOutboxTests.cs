using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;
using Distribt.Shared.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Distribt.Services.Products.BusinessLogic.Tests.UseCases;

public class CreateProductDetailsWithOutboxTests
{
    private readonly Mock<IProductsWriteStore> _mockWriteStore;
    private readonly Mock<IServiceDiscovery> _mockDiscovery;
    private readonly Mock<IStockApi> _mockStockApi;
    private readonly Mock<IWarehouseApi> _mockWarehouseApi;
    private readonly CreateProductDetailsWithOutbox _useCase;

    public CreateProductDetailsWithOutboxTests()
    {
        _mockWriteStore = new Mock<IProductsWriteStore>();
        _mockDiscovery = new Mock<IServiceDiscovery>();
        _mockStockApi = new Mock<IStockApi>();
        _mockWarehouseApi = new Mock<IWarehouseApi>();

        _useCase = new CreateProductDetailsWithOutbox(
            _mockWriteStore.Object,
            _mockDiscovery.Object,
            _mockStockApi.Object,
            _mockWarehouseApi.Object);
    }

    [Fact]
    public async Task Execute_ShouldCreateProductAndStoreOutboxMessage_WhenValidRequest()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"),
            10,
            99.99m);

        var expectedProductId = 123;
        var expectedUrl = "http://localhost:5000";

        _mockWriteStore.Setup(x => x.CreateProductWithOutboxCallback(
                It.IsAny<ProductDetails>(),
                It.IsAny<Func<int, OutboxMessage>>()))
            .ReturnsAsync((expectedProductId, new OutboxMessage()));

        _mockDiscovery.Setup(x => x.GetFullAddress(DiscoveryServices.Microservices.ProductsApi.ApiRead, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        _mockStockApi.Setup(x => x.AddStockToProduct(expectedProductId, 10))
            .ReturnsAsync(true);

        _mockWarehouseApi.Setup(x => x.ModifySalesPrice(expectedProductId, 99.99m))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.Execute(productRequest);

        // Assert
        Assert.Equal($"{expectedUrl}/product/{expectedProductId}", result.Url);
        
        _mockWriteStore.Verify(x => x.CreateProductWithOutboxCallback(
            It.Is<ProductDetails>(pd => pd.Name == "Test Product" && pd.Description == "Test Description"),
            It.IsAny<Func<int, OutboxMessage>>()), Times.Once);

        _mockStockApi.Verify(x => x.AddStockToProduct(expectedProductId, 10), Times.Once);
        _mockWarehouseApi.Verify(x => x.ModifySalesPrice(expectedProductId, 99.99m), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldCreateCorrectOutboxMessage_WhenCalled()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"),
            10,
            99.99m);

        var expectedProductId = 123;
        OutboxMessage? capturedOutboxMessage = null;

        _mockWriteStore.Setup(x => x.CreateProductWithOutboxCallback(
                It.IsAny<ProductDetails>(),
                It.IsAny<Func<int, OutboxMessage>>()))
            .Callback<ProductDetails, Func<int, OutboxMessage>>((_, createOutboxMessageFunc) =>
            {
                capturedOutboxMessage = createOutboxMessageFunc(expectedProductId);
            })
            .ReturnsAsync((expectedProductId, new OutboxMessage()));

        _mockDiscovery.Setup(x => x.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost:5000");

        // Act
        await _useCase.Execute(productRequest);

        // Assert
        Assert.NotNull(capturedOutboxMessage);
        Assert.Equal("ProductCreated", capturedOutboxMessage.EventType);
        Assert.Contains("\"Id\":123", capturedOutboxMessage.EventData);
        Assert.Equal("internal", capturedOutboxMessage.RoutingKey);
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenWriteStoreFails()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"),
            10,
            99.99m);

        _mockWriteStore.Setup(x => x.CreateProductWithOutboxCallback(
                It.IsAny<ProductDetails>(),
                It.IsAny<Func<int, OutboxMessage>>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.Execute(productRequest));
    }
} 