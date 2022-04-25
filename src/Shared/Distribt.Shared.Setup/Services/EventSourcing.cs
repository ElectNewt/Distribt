using Distribt.Shared.EventSourcing;
using Microsoft.Extensions.Configuration;

namespace Distribt.Shared.Setup.Services;

public static class EventSourcing
{
    public static void AddEventSourcing(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMongoEventSourcing(configuration);
    }
}