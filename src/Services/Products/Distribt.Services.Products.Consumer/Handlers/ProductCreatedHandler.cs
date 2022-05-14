using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.Consumer.Handlers;

public class ProductCreatedHandler : IDomainMessageHandler<ProductCreated>
{
    private readonly IProductsReadStore _readStore;
    private readonly IIntegrationMessagePublisher _integrationMessagePublisher;

    public ProductCreatedHandler(IProductsReadStore readStore, IIntegrationMessagePublisher integrationMessagePublisher)
    {
        _readStore = readStore;
        _integrationMessagePublisher = integrationMessagePublisher;
    }


    public async Task Handle(DomainMessage<ProductCreated> message,
        CancellationToken cancelToken = default(CancellationToken))
    {
        await _readStore.UpsertProductViewDetails(message.Content.Id, message.Content.ProductRequest.Details,
            cancelToken);
        await _readStore.UpdateProductStock(message.Content.Id, message.Content.ProductRequest.Stock, cancelToken);
        await _readStore.UpdateProductPrice(message.Content.Id, message.Content.ProductRequest.Price, cancelToken);

        await _integrationMessagePublisher.Publish(
            new ProductUpdated(message.Content.Id, message.Content.ProductRequest.Details), routingKey: "external",
            cancellationToken: cancelToken);
    }
}