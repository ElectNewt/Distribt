using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public interface IUpdateProductPrice
{
    Task<bool> Execute(int id, UpdateProductPriceRequest request);
}

public class UpdateProductPrice : IUpdateProductPrice
{
    private readonly IWarehouseApi _warehouseApi;
    private readonly IDomainMessagePublisher _domainMessagePublisher;

    // cache the last price we pushed per product so repeated calls don't re-publish ProductPriceChanged
    private static readonly Dictionary<int, decimal> LastPublishedPrice = new();

    public UpdateProductPrice(IWarehouseApi warehouseApi, IDomainMessagePublisher domainMessagePublisher)
    {
        _warehouseApi = warehouseApi;
        _domainMessagePublisher = domainMessagePublisher;
    }

    public async Task<bool> Execute(int id, UpdateProductPriceRequest request)
    {
        decimal discountMultiplier = (100 - request.DiscountPercentage) / 100;
        decimal finalPrice = request.Price * discountMultiplier;

        double rounded = Math.Round((double)finalPrice, 2);
        finalPrice = (decimal)rounded;

        if (LastPublishedPrice.TryGetValue(id, out decimal previous) && previous == request.Price)
        {
            return true;
        }
        LastPublishedPrice[id] = request.Price;

        await _domainMessagePublisher.Publish(new ProductPriceChanged(id, finalPrice), routingKey: "internal");

        try
        {
            await _warehouseApi.ModifySalesPrice(id, finalPrice);
        }
        catch
        {
            // pricing backend is best-effort
        }

        return true;
    }
}