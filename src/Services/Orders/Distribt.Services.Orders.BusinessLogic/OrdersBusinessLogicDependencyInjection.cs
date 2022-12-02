using Distribt.Services.Orders.BusinessLogic.Data.External;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Shared.Databases.MongoDb;
using Distribt.Shared.Setup.Databases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Distribt.Services.Orders.BusinessLogic;

public static class OrdersBusinessLogicDependencyInjection
{
    public static async Task AddProductService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        await serviceCollection.AddDistribtMongoDbConnectionProvider(configuration, "productStore");

        var factory = async (IServiceProvider serviceProvider) =>
        {
            var mongoDbConnectionProvider = serviceProvider.GetService<IMongoDbConnectionProvider>() ?? throw new ArgumentNullException();
            var databaseConfiguration = serviceProvider.GetService<IOptions<DatabaseConfiguration>>() ?? throw new ArgumentNullException();
            var mongoUrl = await mongoDbConnectionProvider.GetMongoUrl();
            return new ProductRepository(mongoUrl, databaseConfiguration);
        };
        
        serviceCollection.AddScoped(typeof(IProductRepository), factory);
        serviceCollection.AddScoped<IProductNameService, ProductNameService>();
        serviceCollection.AddHttpClient();
        //For now we do not need redis, as is only for local, in prod I recommend redis.
        serviceCollection.AddDistributedMemoryCache();
    }
    
    
}