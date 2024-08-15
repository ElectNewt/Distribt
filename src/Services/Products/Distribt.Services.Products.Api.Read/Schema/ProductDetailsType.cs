using Distribt.Services.Products.Dtos;
using GraphQL.Types;

namespace Distribt.Services.Products.Api.Read.Schema;

public class ProductDetailsType : ObjectGraphType<ProductDetails>
{
    public ProductDetailsType()
    {
        Name = "ProductDetails";
        Field(x => x.Name);
        Field(x => x.Description);
    }
}