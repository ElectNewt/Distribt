using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic.UseCases;

public class ProductUseCasesIntegrationTests
{
    private ProductsWriteStore CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ProductsWriteStore(options);
    }

    [Fact]
    public async Task CreateProductDetails_ShouldCreateProductAndOutboxMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var mockStockApi = new ProductsDependencyFakeType();
        var mockWarehouseApi = new ProductsDependencyFakeType();
        var mockDiscovery = new MockServiceDiscovery();
        
        var useCase = new CreateProductDetails(dbContext, mockDiscovery, mockStockApi, mockWarehouseApi);
        
        var request = new CreateProductRequest(
            new ProductDetails("Test Product", "Test Description"),
            10,
            99.99m);

        // Act
        var result = await useCase.Execute(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("product/", result.Url);

        // Verify product was created
        var product = await dbContext.Set<ProductDetailEntity>().FirstAsync();
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);

        // Verify outbox message was created
        var outboxMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.Equal(typeof(ProductCreated).AssemblyQualifiedName, outboxMessage.EventType);
        Assert.Equal("internal", outboxMessage.RoutingKey);
        Assert.False(outboxMessage.IsProcessed);
        Assert.Contains("Test Product", outboxMessage.EventData);
    }

    [Fact]
    public async Task UpdateProductDetails_ShouldUpdateProductAndCreateOutboxMessage()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        
        // Create initial product
        var initialProduct = new ProductDetailEntity { Name = "Original", Description = "Original Desc" };
        await dbContext.Set<ProductDetailEntity>().AddAsync(initialProduct);
        await dbContext.SaveChangesAsync();
        
        var useCase = new UpdateProductDetails(dbContext);
        var updatedDetails = new ProductDetails("Updated Product", "Updated Description");

        // Act
        var result = await useCase.Execute(initialProduct.Id!.Value, updatedDetails);

        // Assert
        Assert.True(result);

        // Verify product was updated
        var product = await dbContext.Set<ProductDetailEntity>().FirstAsync();
        Assert.Equal("Updated Product", product.Name);
        Assert.Equal("Updated Description", product.Description);

        // Verify outbox message was created
        var outboxMessage = await dbContext.Set<OutboxMessage>().FirstAsync();
        Assert.Equal(typeof(ProductUpdated).AssemblyQualifiedName, outboxMessage.EventType);
        Assert.Equal("internal", outboxMessage.RoutingKey);
        Assert.False(outboxMessage.IsProcessed);
        Assert.Contains("Updated Product", outboxMessage.EventData);
    }
}

public class MockServiceDiscovery : Distribt.Shared.Discovery.IServiceDiscovery
{
    public Task<string> GetFullAddress(string serviceKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("http://localhost:5000");
    }

    public Task<Distribt.Shared.Discovery.DiscoveryData> GetDiscoveryData(string serviceKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Distribt.Shared.Discovery.DiscoveryData("localhost", 5000));
    }
}