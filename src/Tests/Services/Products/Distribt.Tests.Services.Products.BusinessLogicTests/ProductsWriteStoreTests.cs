using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;

namespace Distribt.Tests.Services.Products.BusinessLogicTests
{
    public class ProductsWriteStoreTests
    {
        private ProductsWriteStore CreateInMemoryProductsWriteStore()
        {
            var options = new DbContextOptionsBuilder<ProductsWriteStore>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var store = new ProductsWriteStore(options);
            store.Database.EnsureCreated(); // Ensure the database is created for each test
            return store;
        }

        [Fact(Skip = "In-memory database does not fully support transactional outbox testing.")]
        public async Task CreateRecord_AddsProductAndOutboxMessageAtomically()
        {
            // Arrange
            var store = CreateInMemoryProductsWriteStore();
            var productDetails = new ProductDetails("Test Product", "Test Description");

            // Act
            var productId = await store.CreateRecord(productDetails);

            // Assert
            // Verify product is created
            var product = await store.Products.SingleOrDefaultAsync(p => p.Id == productId);
            Assert.NotNull(product);
            Assert.Equal(productDetails.Name, product.Name);
            Assert.Equal(productDetails.Description, product.Description);

            // Verify outbox message is created
            var outboxMessage = await store.OutboxMessages.SingleOrDefaultAsync(om => om.ProductId == productId && om.MessageType == "ProductCreated");
            Assert.NotNull(outboxMessage);
            Assert.Contains("Test Product", outboxMessage.Content);
            Assert.Contains("Test Description", outboxMessage.Content);
            Assert.Equal(productId, outboxMessage.ProductId);
        }

        [Fact(Skip = "In-memory database does not fully support transactional outbox testing.")]
        public async Task UpdateProduct_UpdatesProductAndAddsOutboxMessageAtomically()
        {
            // Arrange
            var store = CreateInMemoryProductsWriteStore();
            var initialProductDetails = new ProductDetails("Initial Product", "Initial Description");
            var productId = await store.CreateRecord(initialProductDetails); // Create an initial product

            var updatedProductDetails = new ProductDetails("Updated Product", "Updated Description");

            // Act
            await store.UpdateProduct(productId, updatedProductDetails);

            // Assert
            // Verify product is updated
            var product = await store.Products.SingleOrDefaultAsync(p => p.Id == productId);
            Assert.NotNull(product);
            Assert.Equal(updatedProductDetails.Name, product.Name);
            Assert.Equal(updatedProductDetails.Description, product.Description);

            // Verify outbox message is created for update
            var outboxMessage = await store.OutboxMessages.SingleOrDefaultAsync(om => om.ProductId == productId && om.MessageType == "ProductUpdated");
            Assert.NotNull(outboxMessage);
            Assert.Contains("Updated Product", outboxMessage.Content);
            Assert.Contains("Updated Description", outboxMessage.Content);
            Assert.Equal(productId, outboxMessage.ProductId);
        }
    }
}
