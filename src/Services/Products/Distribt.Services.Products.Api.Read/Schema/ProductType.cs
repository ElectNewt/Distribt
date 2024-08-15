using Distribt.Services.Products.Dtos;
using GraphQL.Types;

namespace Distribt.Services.Products.Api.Read.Schema;

public class ProductType : ObjectGraphType<FullProductResponse>
{
    
    public ProductType()
    {
        Name = "FullProductResponse";
        Field(x => x.Id);
        Field(x => x.Details, type: typeof(ProductDetailsType));
        Field(x => x.Stock);
        Field(x => x.Price);
    }
    
}