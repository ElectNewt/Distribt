using Distribt.Services.Products.Api.Read.Schema;
using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Services.Products.BusinessLogic.UseCases;
using GraphQL;
using GraphQL.Utilities;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration);
    builder.Services.AddScoped<IGetProductById, GetProductById>();
    builder.Services.AddScoped<IProductsReadStore, ProductsReadStore>();


    builder.Services.AddGraphQL(x =>
    {
        x.AddSelfActivatingSchema<ProductReadSchema>();
        x.AddSystemTextJson();
    });
});


app.MapGet("product/{productId}", async (int productId, IProductsReadStore readStore)
    => await readStore.GetFullProduct(productId)); //TODO: result struct gives an error on minimal api?

app.MapGet("graphql-schema", (ProductReadSchema readSchema)
    =>
{
    var schemaPrinter = new SchemaPrinter(readSchema);
    return schemaPrinter.Print();
});

app.UseGraphQL<ProductReadSchema>();

DefaultDistribtWebApplication.Run(app);