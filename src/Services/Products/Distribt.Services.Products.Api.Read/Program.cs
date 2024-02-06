using Distribt.Services.Products.BusinessLogic.DataAccess;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    await builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration);
    builder.Services.AddScoped<IProductsReadStore, ProductsReadStore>();
});


app.MapGet("product/{productId}", async (int productId, IProductsReadStore readStore)
    => await readStore.GetFullProduct(productId)); //TODO: result struct gives an error on minimal api?


DefaultDistribtWebApplication.Run(app);