using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic.UseCases;

public class CreateProductDetailsTests
{
    private readonly Mock<IProductsWriteStore> _mockWriteStore;
    private readonly Mock<IServiceDiscovery> _mockServiceDiscovery;
    private readonly Mock<IStockApi> _mockStockApi;
    private readonly Mock<IWarehouseApi> _mockWarehouseApi;
    private readonly CreateProductDetails _createProductDetails;

    public CreateProductDetailsTests()
    {
        _mockWriteStore = new Mock<IProductsWriteStore>();
        _mockServiceDiscovery = new Mock<IServiceDiscovery>();
        _mockStockApi = new Mock<IStockApi>();
        _mockWarehouseApi = new Mock<IWarehouseApi>();

        _createProductDetails = new CreateProductDetails(
            _mockWriteStore.Object,
            _mockServiceDiscovery.Object,
            _mockStockApi.Object,
            _mockWarehouseApi.Object);
    }

    [Fact]
    public async Task Execute_ShouldCreateProductAndSaveEventInTransaction()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 
            10, 
            99.99m);
        var expectedProductId = 123;
        var expectedUrl = "http://api.read/product/123";

        _mockWriteStore.Setup(x => x.CreateRecord(It.IsAny<ProductDetails>()))
            .ReturnsAsync(expectedProductId);

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(operation => operation.Invoke())
            .Returns(Task.CompletedTask);

        _mockServiceDiscovery.Setup(x => x.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://api.read");

        _mockStockApi.Setup(x => x.AddStockToProduct(expectedProductId, productRequest.Stock))
            .ReturnsAsync(true);

        _mockWarehouseApi.Setup(x => x.ModifySalesPrice(expectedProductId, productRequest.Price))
            .ReturnsAsync(true);

        // Act
        var result = await _createProductDetails.Execute(productRequest);

        // Assert
        Assert.Equal(expectedUrl, result.Url);

        // Verify transaction was used
        _mockWriteStore.Verify(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()), Times.Once);

        // Verify product was created within transaction
        _mockWriteStore.Verify(x => x.CreateRecord(productRequest.Details), Times.Once);

        // Verify outbox message was saved within transaction
        _mockWriteStore.Verify(x => x.SaveOutboxMessage(
            It.Is<ProductCreated>(pc => 
                pc.Id == expectedProductId && 
                pc.ProductRequest.Details.Name == productRequest.Details.Name),
            nameof(ProductCreated)), Times.Once);

        // Verify external services were called
        _mockStockApi.Verify(x => x.AddStockToProduct(expectedProductId, productRequest.Stock), Times.Once);
        _mockWarehouseApi.Verify(x => x.ModifySalesPrice(expectedProductId, productRequest.Price), Times.Once);

        // Note: Domain publisher is no longer injected - outbox pattern handles publishing
    }

    [Fact]
    public async Task Execute_WhenTransactionFails_ShouldNotCallExternalServices()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 
            10, 
            99.99m);

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .ThrowsAsync(new InvalidOperationException("Transaction failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _createProductDetails.Execute(productRequest));

        // Verify external services were NOT called when transaction fails
        _mockStockApi.Verify(x => x.AddStockToProduct(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockWarehouseApi.Verify(x => x.ModifySalesPrice(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldUseCorrectEventData()
    {
        // Arrange
        var productDetails = new ProductDetails("Awesome Product", "Amazing Description");
        var productRequest = new CreateProductRequest(productDetails, 25, 199.99m);
        var expectedProductId = 456;

        _mockWriteStore.Setup(x => x.CreateRecord(It.IsAny<ProductDetails>()))
            .ReturnsAsync(expectedProductId);

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(operation => operation.Invoke())
            .Returns(Task.CompletedTask);

        _mockServiceDiscovery.Setup(x => x.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://api.read");

        ProductCreated? capturedEvent = null;
        _mockWriteStore.Setup(x => x.SaveOutboxMessage(It.IsAny<ProductCreated>(), It.IsAny<string>()))
            .Callback<object, string>((evt, type) => capturedEvent = (ProductCreated)evt);

        // Act
        await _createProductDetails.Execute(productRequest);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(expectedProductId, capturedEvent.Id);
        Assert.Equal(productDetails.Name, capturedEvent.ProductRequest.Details.Name);
        Assert.Equal(productDetails.Description, capturedEvent.ProductRequest.Details.Description);
        Assert.Equal(productRequest.Stock, capturedEvent.ProductRequest.Stock);
        Assert.Equal(productRequest.Price, capturedEvent.ProductRequest.Price);
    }

    [Fact]
    public async Task Execute_ShouldCallServicesInCorrectOrder()
    {
        // Arrange
        var productRequest = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 
            10, 
            99.99m);
        var expectedProductId = 789;
        var callOrder = new List<string>();

        _mockWriteStore.Setup(x => x.CreateRecord(It.IsAny<ProductDetails>()))
            .ReturnsAsync(expectedProductId)
            .Callback(() => callOrder.Add("CreateRecord"));

        _mockWriteStore.Setup(x => x.SaveOutboxMessage(It.IsAny<object>(), It.IsAny<string>()))
            .Callback(() => callOrder.Add("SaveOutboxMessage"));

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(async operation => 
            {
                callOrder.Add("Transaction_Start");
                await operation.Invoke();
                callOrder.Add("Transaction_End");
            })
            .Returns(Task.CompletedTask);

        _mockStockApi.Setup(x => x.AddStockToProduct(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true)
            .Callback(() => callOrder.Add("StockApi"));

        _mockWarehouseApi.Setup(x => x.ModifySalesPrice(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(true)
            .Callback(() => callOrder.Add("WarehouseApi"));

        _mockServiceDiscovery.Setup(x => x.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://api.read")
            .Callback(() => callOrder.Add("ServiceDiscovery"));

        // Act
        await _createProductDetails.Execute(productRequest);

        // Assert
        var expectedOrder = new[]
        {
            "Transaction_Start",
            "CreateRecord", 
            "SaveOutboxMessage",
            "Transaction_End",
            "StockApi",
            "WarehouseApi",
            "ServiceDiscovery"
        };

        Assert.Equal(expectedOrder, callOrder);
    }
}
