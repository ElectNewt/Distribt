using Distribt.Services.Orders.Events;
using MongoDB.Bson.Serialization;

namespace Distribt.Services.Orders.Aggregates;

public static class MongoMapping
{
    public static void RegisterClasses()
    {
        //#22 find a way to register the classes automatically or avoid the registration
        BsonClassMap.RegisterClassMap<OrderCreated>();
        BsonClassMap.RegisterClassMap<OrderPaid>();
        BsonClassMap.RegisterClassMap<OrderDispatched>();
    }
}