using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using GraphQL;
using GraphQL.Types;

namespace Distribt.Services.Products.Api.Read.Schema;

public class ProductQuery : ObjectGraphType<object>
{
  
    public ProductQuery()
    {
        Field<ProductType>("FullProductResponse")
            .Description("Get a full product by ID")
            .Arguments(new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }))
            .ResolveAsync(async ctx =>
            {
                var id = ctx.GetArgument<int>("id");
                IGetProductById getById = ctx.RequestServices!.GetRequiredService<IGetProductById>();
                return await getById.Execute(id);
            })
            .Authorize()
            .AuthorizeWithPolicy("policy1")
            .AuthorizeWithRoles("role");
    }
}