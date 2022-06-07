using Distribt.Shared.Discovery;
using Distribt.Shared.Secrets;
using MongoDB.Driver;

namespace Distribt.Shared.Databases.MongoDb;

public interface IMongoDbConnectionProvider
{
    MongoUrl GetMongoUrl();
    string GetMongoConnectionString();
}

public class MongoDbConnectionProvider : IMongoDbConnectionProvider
{
    private readonly ISecretManager _secretManager;
    private readonly IServiceDiscovery _serviceDiscovery;

    private MongoUrl? MongoUrl { get; set; }
    private string? MongoConnectionString { get; set; }

    public MongoDbConnectionProvider(ISecretManager secretManager, IServiceDiscovery serviceDiscovery)
    {
        _secretManager = secretManager;
        _serviceDiscovery = serviceDiscovery;
    }


    public MongoUrl GetMongoUrl()
    {
        if (MongoUrl is not null)
            return MongoUrl;

        MongoConnectionString = RetrieveMongoUrl().Result;
        MongoUrl = new MongoUrl(MongoConnectionString);

        return MongoUrl;
    }

    public string GetMongoConnectionString()
    {
        if (MongoConnectionString is null)
            GetMongoUrl();

        return MongoConnectionString ?? throw new Exception("Mongo connection string cannot be retrieved");
    }

    private async Task<string> RetrieveMongoUrl()
    {
        DiscoveryData mongoData = await _serviceDiscovery.GetDiscoveryData(DiscoveryServices.MongoDb);
        MongoDbCredentials credentials = await _secretManager.Get<MongoDbCredentials>("mongodb");

        return $"mongodb://{credentials.username}:{credentials.password}@{mongoData.Server}:{mongoData.Port}";
    }


    private record MongoDbCredentials
    {
        public string username { get; init; } = null!;
        public string password { get; init; } = null!;
    }
}