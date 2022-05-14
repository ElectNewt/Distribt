using Distribt.Services.Products.BusinessLogic.DataAccess;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration)
        .AddScoped<IProductsReadStore, ProductsReadStore>();
});


app.MapGet("product/{productId}", async (int productId, IProductsReadStore readStore)
    => await readStore.GetFullProduct(productId)); //TODO: result struct gives an error on minimal api?


DefaultDistribtWebApplication.Run(app);