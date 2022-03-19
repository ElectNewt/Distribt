using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Distribt.Shared.Databases.MongoDb;

public static class MongoDbDependencyInjection
{
    public static IServiceCollection AddMongoDbConnectionProvider<T>(this IServiceCollection serviceCollection, T mongoDbConnectionProvider)
    where T : IMongoDbConnectionProvider
    {
        throw new NotImplementedException("#20");
    }
}

public interface IMongoDbConnectionProvider
{
    MongoUrl GetMongoUrl();
}
