using Distribt.Services.Orders.BusinessLogic.Data.External;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Shared.Setup.Databases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Services.Orders.BusinessLogic;

public static class OrdersBusinessLogicDependencyInjection
{
    public static void AddProductService(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddDistribtMongoDbConnectionProvider(configuration, "productStore");
        serviceCollection.AddScoped<IProductRepository, ProductRepository>();
        serviceCollection.AddScoped<IProductNameService, ProductNameService>();
        serviceCollection.AddHttpClient();
        //For now we do not need redis, as is only for local, in prod I recommend redis.
        serviceCollection.AddDistributedMemoryCache();
    }
    
    
}