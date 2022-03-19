using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;
using Distribt.Shared.Communication.Publisher.Domain;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public interface IUpdateProductDetails
{
    Task<bool> Execute(int id, ProductDetails productDetails);
}

public class UpdateProductDetails : IUpdateProductDetails
{
    private readonly IProductsWriteStore _writeStore;
    private readonly IDomainMessagePublisher _domainMessagePublisher;

    public UpdateProductDetails(IProductsWriteStore writeStore, IDomainMessagePublisher domainMessagePublisher)
    {
        _writeStore = writeStore;
        _domainMessagePublisher = domainMessagePublisher;
    }

    public async Task<bool> Execute(int id, ProductDetails productDetails)
    {
        await _writeStore.UpdateProduct(id, productDetails);

        await _domainMessagePublisher.Publish(new ProductUpdated(id, productDetails), routingKey: "internal");
        
        return true;
    }
}