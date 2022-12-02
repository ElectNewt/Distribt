using Distribt.Services.Products.BusinessLogic.DataAccess;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;

WebApplication app = await DefaultDistribtWebApplication.Create(args, async builder =>
{
    var factory = async (IServiceProvider serviceProvider) =>
    {
        var mongoDbConnectionProvider = serviceProvider.GetService<IMongoDbConnectionProvider>() ?? throw new ArgumentNullException();
        var databaseConfiguration = serviceProvider.GetService<IOptions<DatabaseConfiguration>>() ?? throw new ArgumentNullException();
        var mongoUrl = await mongoDbConnectionProvider.GetMongoUrl();
        return new ProductsReadStore(mongoUrl, databaseConfiguration);
    };
    (await builder.Services.AddDistribtMongoDbConnectionProvider(builder.Configuration))
        .AddScoped(typeof(IProductsReadStore), factory);
});


app.MapGet("product/{productId}", async (int productId, IProductsReadStore readStore)
    => await readStore.GetFullProduct(productId)); //TODO: result struct gives an error on minimal api?


DefaultDistribtWebApplication.Run(app);