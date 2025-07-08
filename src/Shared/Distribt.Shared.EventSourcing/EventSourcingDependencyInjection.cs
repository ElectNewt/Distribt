using Distribt.Shared.Databases.MongoDb;
using Distribt.Shared.EventSourcing.EventStores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Distribt.Shared.EventSourcing;

public static class EventSourcingDependencyInjection
{
    public static void AddMongoEventSourcing(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        //TODO: probably here it should be the addmongodb thingy
        serviceCollection.AddTransient(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
        serviceCollection.AddTransient<IEventStore, EventStore>();
        serviceCollection.AddTransient<IEventStoreManager>((IServiceProvider serviceProvider) =>
        {
            var mongoDbConnectionProvider = serviceProvider.GetService<IMongoDbConnectionProvider>()
                                            ?? throw new ArgumentNullException(nameof(IMongoDbConnectionProvider));
            var mongoDbEventStoreOptions = serviceProvider.GetService<IOptions<MongoEventStoreConfiguration>>()
                                           ?? throw new ArgumentNullException(nameof(IOptions<MongoEventStoreConfiguration>));
            var mongoUrl = mongoDbConnectionProvider.GetMongoUrl().GetAwaiter().GetResult();
            return new MongoEventStoreManager(mongoDbEventStoreOptions, mongoUrl);
        });
        serviceCollection.Configure<MongoEventStoreConfiguration>(configuration.GetSection("EventSourcing"));
    }
}