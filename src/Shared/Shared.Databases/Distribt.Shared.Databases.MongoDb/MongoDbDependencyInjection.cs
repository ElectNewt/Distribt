using Microsoft.Extensions.DependencyInjection;
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




//public record EventStoreOptions(string DatabaseName, string CollectionName);
//nullables are a pain in the ass for the configuration files
public class EventStoreOptions
{
    public string DatabaseName { get; set; } = default!;
    public string CollectionName { get; set; } = default!;
}
