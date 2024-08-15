namespace Distribt.Services.Products.Api.Read.Schema;

public class ProductReadSchema : GraphQL.Types.Schema
{
    public ProductReadSchema(ProductQuery query)
    {
        Query = query;
    }
}