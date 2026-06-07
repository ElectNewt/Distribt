using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.Consumer.Handlers;

public class ProductPriceChangedHandler(
    IProductsReadStore readStore,
    IIntegrationMessagePublisher integrationMessagePublisher)
    : IDomainMessageHandler<ProductPriceChanged>
{
    public async Task Handle(DomainMessage<ProductPriceChanged> message, CancellationToken cancellationToken = default(CancellationToken))
    {
        await readStore.UpdateProductPrice(message.Content.ProductId, message.Content.Price);

        await integrationMessagePublisher.Publish(
            new ProductPriceChanged(message.Content.ProductId, message.Content.Price),
            routingKey: "external", cancellationToken: cancellationToken);
    }
}