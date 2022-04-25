using Distribt.Shared.Databases.MongoDb;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Distribt.Shared.Setup.Databases;

public static class MongoDb
{
    public static IServiceCollection AddDistribtMongoDbConnectionProvider(this IServiceCollection serviceCollection)
    {
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        
        return serviceCollection.AddScoped<IMongoDbConnectionProvider, MongoDbConnectionProvider>(); 
    }

}



//TODO: #20
public class MongoDbConnectionProvider : IMongoDbConnectionProvider
{
    private readonly ISecretManager _secretManager;
    private readonly IServiceDiscovery _serviceDiscovery;
    
    private MongoUrl? MongoUrl { get; set; }

    public MongoDbConnectionProvider(ISecretManager secretManager, IServiceDiscovery serviceDiscovery)
    {
        _secretManager = secretManager;
        _serviceDiscovery = serviceDiscovery;
    }


    public MongoUrl GetMongoUrl()
    {
        if (MongoUrl is not null)
            return MongoUrl;

        string mongoConnection = RetrieveMongoUrl().Result;
        MongoUrl = new MongoUrl(mongoConnection);

        return MongoUrl;
    }

    private async Task<string> RetrieveMongoUrl()
    {
        DiscoveryData mongoData = await _serviceDiscovery.GetDiscoveryData(DiscoveryServices.MongoDb);
        MongoDbCredentials credentials =await _secretManager.Get<MongoDbCredentials>("mongodb");

        return $"mongodb://{credentials.username}:{credentials.password}@{mongoData.Server}:{mongoData.Port}";
    }
    
    
    private record MongoDbCredentials
    {
        public string username { get; init; } = null!;
        public string password { get; init; } = null!;
    }
}