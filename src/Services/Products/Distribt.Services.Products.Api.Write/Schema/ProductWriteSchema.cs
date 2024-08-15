using Distribt.Services.Products.BusinessLogic.UseCases;
using Distribt.Services.Products.Dtos;
using GraphQL;
using GraphQL.Types;

namespace Distribt.Services.Products.Api.Write.Schema;

public class ProductWriteSchema : GraphQL.Types.Schema
{
    public ProductWriteSchema(ProductMutation mutation, ProductQuery query)
    {
        Mutation = mutation;
        Query = query;
    }
}

public class ProductQuery : ObjectGraphType<object>
{
    public ProductQuery()
    {
        Field<StringGraphType>("info")
            .Resolve(_ => "Hello World");
    }
}

public class ProductMutation : ObjectGraphType<object>
{
    public ProductMutation()
    {
        Field<CreateProductResponseType>("CreateProduct")
            .Description("Create a product in the system")
            .Arguments(new QueryArguments(new QueryArgument<CreateProductRequestType> { Name = "product" }))
            .ResolveAsync(async ctx =>
            {
                var product = ctx.GetArgument<CreateProductRequest>("product");
                ICreateProductDetails createProduct = ctx.RequestServices!.GetRequiredService<ICreateProductDetails>();
                return await createProduct.Execute(product);
            });
    }
}

public class CreateProductResponseType : ObjectGraphType<CreateProductResponse>
{
    public CreateProductResponseType()
    {
        Name = "FullProductResponse";
        Field(x => x.Url);
    }
}

public class ProductDetailsType : InputObjectGraphType<ProductDetails>
{
    public ProductDetailsType()
    {
        Name = "ProductDetails";
        Field(x => x.Name);
        Field(x => x.Description);
    }
}

public class CreateProductRequestType : InputObjectGraphType<CreateProductRequest>
{
    public CreateProductRequestType()
    {
        Name = "CreateProductRequest";
        Field(x => x.Details, type: typeof(ProductDetailsType));
        Field(x => x.Stock);
        Field(x => x.Price);
    }
}