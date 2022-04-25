using Distribt.Shared.Databases.MongoDb;
using Distribt.Shared.EventSourcing.EventStores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.EventSourcing;

public static class EventSourcingDependencyInjection
{
    public static void AddMongoEventSourcing(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
        serviceCollection.AddTransient<IEventStore, EventStore>();
        serviceCollection.AddTransient<IEventStoreManager, MongoEventStoreManager>();
        serviceCollection.Configure<EventStoreOptions>(configuration.GetSection("EventSourcing"));
    }
}