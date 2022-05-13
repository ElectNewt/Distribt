using System.Net;
using Distribt.Services.Products.BusinessLogic.DataAccess;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddDistribtMongoDbConnectionProvider()
        .AddScoped<IProductsReadStore, ProductsReadStore>();
});


app.MapGet("product/{productId}", async (int productId, IProductsReadStore readStore)
    => (await readStore.GetFullProduct(productId)) //this is a demo, it should not be calling the read store directly
    .Success()
    .Async()
    .UseSuccessHttpStatusCode(HttpStatusCode.OK)
    .ToActionResult());


DefaultDistribtWebApplication.Run(app);