using Distribt.Shared.Databases.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Distribt.Services.Orders.BusinessLogic.Data.External;

public interface IProductRepository
{
    Task<string?> GetProductName(int id, CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> UpsertProductName(int id, string name, CancellationToken cancellationToken = default(CancellationToken));
}

public class ProductRepository : IProductRepository
{
    private readonly MongoClient _mongoClient;

    public ProductRepository(IMongoDbConnectionProvider mongoDbConnectionProvider)
    {
        _mongoClient = new MongoClient(mongoDbConnectionProvider.GetMongoUrl());
    }


    public async Task<string?> GetProductName(int id, CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoDatabase db = _mongoClient.GetDatabase("distribt");

        IMongoCollection<ProductNameEntity>
            collection = db.GetCollection<ProductNameEntity>("ProductName");
        FilterDefinition<ProductNameEntity> filter = Builders<ProductNameEntity>.Filter.Eq("Id", id);
        ProductNameEntity entity = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return entity?.Name;
    }

    public async Task<bool> UpsertProductName(int id, string name,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoDatabase db = _mongoClient.GetDatabase("distribt");
        IMongoCollection<ProductNameEntity>
            collection = db.GetCollection<ProductNameEntity>("ProductName");

        FilterDefinition<ProductNameEntity> filter = Builders<ProductNameEntity>.Filter.Eq("Id", id);

        ProductNameEntity entity =
            await collection.Find(filter).FirstOrDefaultAsync(cancellationToken) 
            ?? new ProductNameEntity();

        entity.Id ??= id;
        entity.Name = name;

        var replaceOne = await collection.ReplaceOneAsync(filter,
            entity,
            new ReplaceOptions()
            {
                IsUpsert = true
            }, cancellationToken);

        return replaceOne.IsAcknowledged;
    }

    private class ProductNameEntity
    {
        [BsonId] public ObjectId _Id { get; set; }
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}