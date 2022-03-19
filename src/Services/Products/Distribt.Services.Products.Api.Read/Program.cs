using Distribt.Services.Products.BusinessLogic.DataAccess;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddDistribtMongoDbConnectionProvider()
        .AddScoped<IProductsReadStore, ProductsReadStore>();
});


app.MapGet("product/{productId}", async ( int productId, IProductsReadStore readStore) 
    => await readStore.GetFullProduct(productId));


DefaultDistribtWebApplication.Run(app);