using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Benchmarks.Products
{
    [MemoryDiagnoser]
    public class ProductsWriteStoreBenchmarks
    {
        private static DbContextOptions<ProductsWriteStore> NewOptions()
            => new DbContextOptionsBuilder<ProductsWriteStore>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Benchmark]
        public async Task CreateRecord_Isolated()
        {
            var options = NewOptions();
            await using var store = new ProductsWriteStore(options);

            var details = new ProductDetails(
                Name: "Product-" + Guid.NewGuid().ToString("N"),
                Description: "Desc-" + Guid.NewGuid().ToString("N")
            );

            _ = await store.CreateRecord(details);
        }

        [Benchmark]
        public async Task UpdateProduct_Isolated()
        {
            var options = NewOptions();
            await using var store = new ProductsWriteStore(options);

            // Seed a product to update in this isolated database
            var existingId = await store.CreateRecord(new ProductDetails(
                Name: "Seed-" + Guid.NewGuid().ToString("N"),
                Description: "Seed-Desc"
            ));

            var updateDetails = new ProductDetails(
                Name: "Updated-" + Guid.NewGuid().ToString("N"),
                Description: "Updated-Desc"
            );

            await store.UpdateProduct(existingId, updateDetails);
        }
    }
}