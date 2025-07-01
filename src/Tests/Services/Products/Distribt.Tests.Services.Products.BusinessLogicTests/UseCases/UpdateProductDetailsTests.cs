using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogic.UseCases;

public class UpdateProductDetailsTests
{
    private readonly Mock<IProductsWriteStore> _mockWriteStore;
    private readonly UpdateProductDetails _updateProductDetails;

    public UpdateProductDetailsTests()
    {
        _mockWriteStore = new Mock<IProductsWriteStore>();

        _updateProductDetails = new UpdateProductDetails(
            _mockWriteStore.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdateProductAndSaveEventInTransaction()
    {
        // Arrange
        var productId = 123;
        var productDetails = new ProductDetails("Updated Product", "Updated Description");

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(operation => operation.Invoke())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _updateProductDetails.Execute(productId, productDetails);

        // Assert
        Assert.True(result);

        // Verify transaction was used
        _mockWriteStore.Verify(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()), Times.Once);

        // Verify product was updated within transaction
        _mockWriteStore.Verify(x => x.UpdateProduct(productId, productDetails), Times.Once);

        // Verify outbox message was saved within transaction
        _mockWriteStore.Verify(x => x.SaveOutboxMessage(
            It.Is<ProductUpdated>(pu => 
                pu.ProductId == productId && 
                pu.Details.Name == productDetails.Name &&
                pu.Details.Description == productDetails.Description),
            nameof(ProductUpdated)), Times.Once);

        // Note: Domain publisher is no longer injected - outbox pattern handles publishing
    }

    [Fact]
    public async Task Execute_WhenTransactionFails_ShouldPropagateException()
    {
        // Arrange
        var productId = 123;
        var productDetails = new ProductDetails("Updated Product", "Updated Description");
        var expectedException = new InvalidOperationException("Database transaction failed");

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _updateProductDetails.Execute(productId, productDetails));

        Assert.Equal(expectedException.Message, actualException.Message);

        // Verify transaction was attempted
        _mockWriteStore.Verify(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldUseCorrectEventData()
    {
        // Arrange
        var productId = 456;
        var productDetails = new ProductDetails("Premium Product", "High-quality Description");

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(operation => operation.Invoke())
            .Returns(Task.CompletedTask);

        ProductUpdated? capturedEvent = null;
        _mockWriteStore.Setup(x => x.SaveOutboxMessage(It.IsAny<ProductUpdated>(), It.IsAny<string>()))
            .Callback<object, string>((evt, type) => capturedEvent = (ProductUpdated)evt);

        // Act
        await _updateProductDetails.Execute(productId, productDetails);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(productId, capturedEvent.ProductId);
        Assert.Equal(productDetails.Name, capturedEvent.Details.Name);
        Assert.Equal(productDetails.Description, capturedEvent.Details.Description);
    }

    [Fact]
    public async Task Execute_ShouldPerformOperationsInCorrectOrder()
    {
        // Arrange
        var productId = 789;
        var productDetails = new ProductDetails("Test Product", "Test Description");
        var callOrder = new List<string>();

        _mockWriteStore.Setup(x => x.UpdateProduct(It.IsAny<int>(), It.IsAny<ProductDetails>()))
            .Callback(() => callOrder.Add("UpdateProduct"));

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

        // Act
        await _updateProductDetails.Execute(productId, productDetails);

        // Assert
        var expectedOrder = new[]
        {
            "Transaction_Start",
            "UpdateProduct",
            "SaveOutboxMessage",
            "Transaction_End"
        };

        Assert.Equal(expectedOrder, callOrder);
    }

    [Fact]
    public async Task Execute_WithDifferentProductDetails_ShouldSaveCorrectEvent()
    {
        // Arrange
        var testCases = new[]
        {
            new { Id = 1, Details = new ProductDetails("Product A", "Description A") },
            new { Id = 2, Details = new ProductDetails("Product B", "Description B") },
            new { Id = 3, Details = new ProductDetails("Product C", "Description C") }
        };

        var capturedEvents = new List<ProductUpdated>();

        _mockWriteStore.Setup(x => x.ExecuteInTransaction(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(operation => operation.Invoke())
            .Returns(Task.CompletedTask);

        _mockWriteStore.Setup(x => x.SaveOutboxMessage(It.IsAny<ProductUpdated>(), It.IsAny<string>()))
            .Callback<object, string>((evt, type) => capturedEvents.Add((ProductUpdated)evt));

        // Act
        foreach (var testCase in testCases)
        {
            await _updateProductDetails.Execute(testCase.Id, testCase.Details);
        }

        // Assert
        Assert.Equal(3, capturedEvents.Count);
        
        for (int i = 0; i < testCases.Length; i++)
        {
            Assert.Equal(testCases[i].Id, capturedEvents[i].ProductId);
            Assert.Equal(testCases[i].Details.Name, capturedEvents[i].Details.Name);
            Assert.Equal(testCases[i].Details.Description, capturedEvents[i].Details.Description);
        }
    }
}
