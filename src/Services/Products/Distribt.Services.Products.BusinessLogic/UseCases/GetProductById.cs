using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.Dtos;

namespace Distribt.Services.Products.BusinessLogic.UseCases;

public interface IGetProductById
{
    Task<FullProductResponse> Execute(int id);
}

public class GetProductById(IProductsReadStore readStore) : IGetProductById
{
    public async Task<FullProductResponse> Execute(int id)
        => await readStore.GetFullProduct(id);
}