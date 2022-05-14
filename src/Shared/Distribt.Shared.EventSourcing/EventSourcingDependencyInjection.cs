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
        serviceCollection.AddTransient<IEventStoreManager, MongoEventStoreManager>();
        serviceCollection.Configure<MongoEventStoreConfiguration>(configuration.GetSection("EventSourcing"));
    }
}