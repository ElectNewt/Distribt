using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.Consumer.Handlers;

public class ProductUpdatedHandler : IDomainMessageHandler<ProductUpdated>
{
    private readonly IProductsReadStore _readStore;
    private readonly IIntegrationMessagePublisher _integrationMessagePublisher;

    public ProductUpdatedHandler(IProductsReadStore readStore, IIntegrationMessagePublisher integrationMessagePublisher)
    {
        _readStore = readStore;
        _integrationMessagePublisher = integrationMessagePublisher;
    }

    public async Task Handle(DomainMessage<ProductUpdated> message, CancellationToken cancelToken = default(CancellationToken))
    {

        await _readStore.UpsertProductViewDetails(message.Content.ProductId, message.Content.Details, cancelToken);

        await _integrationMessagePublisher.Publish(
            new ProductUpdated(message.Content.ProductId, message.Content.Details), routingKey:"external", cancellationToken: cancelToken);
    }
}