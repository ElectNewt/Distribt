using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Discovery;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic;

public class CreateProductDetailsTests
{
    private ProductsWriteStore CreateInMemoryStore()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductsWriteStore(options);
    }

    [Fact]
    public async Task Execute_ShouldCreateProductAndAddOutboxMessage()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var mockDiscovery = new Mock<IServiceDiscovery>();
        var mockStockApi = new Mock<IStockApi>();
        var mockWarehouseApi = new Mock<IWarehouseApi>();

        mockDiscovery.Setup(x => x.GetFullAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync("http://localhost:5000");
        mockStockApi.Setup(x => x.AddStockToProduct(It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync(true);
        mockWarehouseApi.Setup(x => x.ModifySalesPrice(It.IsAny<int>(), It.IsAny<decimal>()))
                       .ReturnsAsync(true);

        var createProductDetails = new CreateProductDetails(
            store, 
            mockDiscovery.Object, 
            mockStockApi.Object, 
            mockWarehouseApi.Object);

        var request = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 
            10, 
            99.99m);

        var result = await createProductDetails.Execute(request);

        Assert.NotNull(result);
        Assert.Contains("http://localhost:5000/product/", result.Url);

        var outboxMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Single(outboxMessages);
        Assert.Equal(nameof(ProductCreated), outboxMessages.First().EventType);
        Assert.Equal("internal", outboxMessages.First().RoutingKey);

    }

    [Fact]
    public async Task Execute_ShouldCreateOutboxMessageEvenWhenExternalServiceFails()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var mockDiscovery = new Mock<IServiceDiscovery>();
        var mockStockApi = new Mock<IStockApi>();
        var mockWarehouseApi = new Mock<IWarehouseApi>();

        mockStockApi.Setup(x => x.AddStockToProduct(It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("Stock API failed"));

        var createProductDetails = new CreateProductDetails(
            store, 
            mockDiscovery.Object, 
            mockStockApi.Object, 
            mockWarehouseApi.Object);

        var request = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"), 
            10, 
            99.99m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => createProductDetails.Execute(request));

        var outboxMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Single(outboxMessages);
        Assert.Equal(nameof(ProductCreated), outboxMessages.First().EventType);
    }
}