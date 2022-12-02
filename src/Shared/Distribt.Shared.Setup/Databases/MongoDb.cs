using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Configuration;

namespace Distribt.Shared.Setup.Databases;

public static class MongoDb
{
    public static async Task<IServiceCollection> AddDistribtMongoDbConnectionProvider(this IServiceCollection serviceCollection,
        IConfiguration configuration, string name = "mongodb")
    {
        return await (serviceCollection
            .AddMongoDbConnectionProvider())
            .AddMongoDbDatabaseConfiguration(configuration)
            .AddMongoHealthCheck(name);
    }
}