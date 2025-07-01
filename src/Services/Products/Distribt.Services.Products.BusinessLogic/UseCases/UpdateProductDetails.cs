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

    public UpdateProductDetails(IProductsWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<bool> Execute(int id, ProductDetails productDetails)
    {
        await _writeStore.UpdateProduct(id, productDetails);
        
        return true;
    }
}