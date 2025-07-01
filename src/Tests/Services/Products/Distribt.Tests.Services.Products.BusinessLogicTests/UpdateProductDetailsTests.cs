using System;
using System.Linq;
using System.Threading.Tasks;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic;

public class UpdateProductDetailsTests
{
    private ProductsWriteStore CreateInMemoryStore()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductsWriteStore(options);
    }

    [Fact]
    public async Task Execute_ShouldUpdateProductAndAddOutboxMessage()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var productDetails = new ProductDetails("Original Product", "Original Description");
        var productId = await store.CreateRecord(productDetails);
        await store.SaveChangesAsync();

        var updateProductDetails = new UpdateProductDetails(store);

        var updatedDetails = new ProductDetails("Updated Product", "Updated Description");
        var result = await updateProductDetails.Execute(productId, updatedDetails);

        Assert.True(result);

        var outboxMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Single(outboxMessages);
        Assert.Equal(nameof(ProductUpdated), outboxMessages.First().EventType);
        Assert.Equal("internal", outboxMessages.First().RoutingKey);

    }

    [Fact]
    public async Task Execute_ShouldRollbackTransactionOnFailure()
    {
        using var store = CreateInMemoryStore();
        await store.Database.EnsureCreatedAsync();

        var updateProductDetails = new UpdateProductDetails(store);

        var updatedDetails = new ProductDetails("Updated Product", "Updated Description");
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            updateProductDetails.Execute(999, updatedDetails));

        var outboxMessages = await store.GetUnprocessedOutboxMessages();
        Assert.Empty(outboxMessages);
    }
}