using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Distribt.Services.Products.BusinessLogic.BackgroundServices;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Messages;
using Distribt.Shared.Communication.Publisher.Domain;
using Distribt.Shared.Discovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Distribt.Tests.Services.Products.BusinessLogicTests;

public class OutboxTests
{
    [Fact]
    public async Task WhenCreateProduct_ThenOutboxMessageIsCreated()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ProductsWriteStore(options);
        var useCase = new CreateProductDetails(context, new Mock<IServiceDiscovery>().Object, new Mock<IStockApi>().Object, new Mock<IWarehouseApi>().Object);

        await useCase.Execute(new CreateProductRequest(new ProductDetails("p","d"),1,1));

        Assert.Equal(1, await context.Set<OutboxMessageEntity>().CountAsync());
    }

    [Fact]
    public async Task WhenUpdateProduct_ThenOutboxMessageIsCreated()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ProductsWriteStore(options);
        int id = await context.CreateRecord(new ProductDetails("p","d"));
        var useCase = new UpdateProductDetails(context);

        await useCase.Execute(id, new ProductDetails("n","d"));

        Assert.Equal(1, await context.Set<OutboxMessageEntity>().CountAsync());
    }

    [Fact]
    public async Task OutboxProcessor_Publishes_AndMarksSent()
    {
        var options = new DbContextOptionsBuilder<ProductsWriteStore>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ProductsWriteStore(options);
        context.Add(new OutboxMessageEntity
        {
            Type = typeof(ProductUpdated).AssemblyQualifiedName!,
            Payload = System.Text.Json.JsonSerializer.Serialize(new ProductUpdated(1, new ProductDetails("n","d"))),
            CreatedUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var publisher = new FakePublisher();
        var services = new ServiceCollection();
        services.AddSingleton<IProductsWriteStore>(context);
        services.AddSingleton<IDomainMessagePublisher>(publisher);
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var logger = provider.GetRequiredService<ILogger<OutboxProcessor>>();
        var processor = new OutboxProcessor(scopeFactory, logger);

        using var cts = new CancellationTokenSource(500);
        await processor.StartAsync(cts.Token);
        await Task.Delay(200);
        await processor.StopAsync(CancellationToken.None);

        Assert.Single(publisher.Published);
        Assert.NotNull(context.Set<OutboxMessageEntity>().First().SentUtc);
    }

    private class FakePublisher : IDomainMessagePublisher
    {
        public readonly System.Collections.Generic.List<object> Published = new();
        public Task Publish(object message, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default)
        {
            Published.Add(message);
            return Task.CompletedTask;
        }
        public Task PublishMany(System.Collections.Generic.IEnumerable<object> messages, Metadata? metadata = null, string? routingKey = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
