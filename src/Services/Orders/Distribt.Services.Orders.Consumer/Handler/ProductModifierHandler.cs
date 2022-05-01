using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Orders.Consumer.Handler;

public class ProductModifierHandler : IIntegrationMessageHandler<ProductUpdated>
{
    private readonly IProductNameService _productNameService;

    public ProductModifierHandler(IProductNameService productNameService)
    {
        _productNameService = productNameService;
    }

    public async Task Handle(IntegrationMessage<ProductUpdated> message, CancellationToken cancelToken = default(CancellationToken))
    {
        await _productNameService.SetProductName(message.Content.ProductId, message.Content.Details.Name, cancelToken);
    }
}