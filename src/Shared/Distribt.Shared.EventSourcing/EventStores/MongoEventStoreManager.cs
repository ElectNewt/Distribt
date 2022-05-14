using System.Data;
using Distribt.Shared.Databases.MongoDb;
using Distribt.Shared.EventSourcing.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Distribt.Shared.EventSourcing.EventStores;

public class MongoEventStoreManager : IEventStoreManager
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly MongoEventStoreConfiguration _mongoDbMongoEventStoreConfiguration;

    private IMongoCollection<AggregateChangeDto> _changes =>
        _mongoDatabase.GetCollection<AggregateChangeDto>(_mongoDbMongoEventStoreConfiguration.CollectionName);

    
    public MongoEventStoreManager(IMongoDbConnectionProvider mongoDbConnectionProvider, IOptions<MongoEventStoreConfiguration> mongoDbEventStoreOptions)
    {
        _mongoDbMongoEventStoreConfiguration = mongoDbEventStoreOptions.Value;
        //TODO: #29; investigate the usage of IMongoDatabase
        var mongoClient = new MongoClient(mongoDbConnectionProvider.GetMongoUrl());
        _mongoDatabase = mongoClient.GetDatabase(_mongoDbMongoEventStoreConfiguration.DatabaseName);;
        
    }


    public async Task SaveEvents(Guid id, string aggregateType, IEnumerable<AggregateChange> events, int expectedVersion, CancellationToken cancellationToken = default(CancellationToken))
    {
        var collection = _changes;
        await CreateIndex(collection);
        var latestAggregate = await collection
            .Find(d => d.AggregateType == aggregateType && d.AggregateId == id)
            .SortByDescending(d => d.AggregateVersion)
            .Limit(1)
            .FirstOrDefaultAsync(cancellationToken);
        var latestAggregateVersion = latestAggregate?.AggregateVersion;

        if (latestAggregateVersion.HasValue && latestAggregateVersion >= expectedVersion)
            throw new DBConcurrencyException("Concurrency exception");

        var dtos = events.Select(x =>
            AggregateMappers.ToTypedAggregateChangeDto(id, aggregateType, x)
        );

        await collection.InsertManyAsync(dtos, new InsertManyOptions() { IsOrdered = true }, cancellationToken);
    }

    public async Task<IEnumerable<AggregateChange>> GetEventsForAggregate(string aggregateType, Guid id, CancellationToken cancellationToken = default(CancellationToken))
    {
        List<AggregateChangeDto>? result = await _changes
            .Find(aggregate => aggregate.AggregateType == aggregateType && aggregate.AggregateId == id)
            .SortBy(a => a.AggregateVersion)
            .ToListAsync(cancellationToken);

        return result.Select(AggregateMappers.ToAggregateChange);
    }
    
    private static async Task CreateIndex(IMongoCollection<AggregateChangeDto> collection)
    {
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<AggregateChangeDto>(
                Builders<AggregateChangeDto>.IndexKeys
                    .Ascending(i => i.AggregateType)
                    .Ascending(i => i.AggregateId)
                    .Ascending(i => i.AggregateVersion),
                new CreateIndexOptions { Unique = true, Name = "_Aggregate_Type_Id_Version_" }))
            .ConfigureAwait(false);
    }
}