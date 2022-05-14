using Distribt.Services.Products.Dtos;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public interface IProductsReadStore
{
    Task<FullProductResponse> GetFullProduct(int id, CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> UpsertProductViewDetails(int id, ProductDetails details,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> UpdateProductStock(int id, int stock,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> UpdateProductPrice(int id, decimal price,
        CancellationToken cancellationToken = default(CancellationToken));
}

public class ProductsReadStore : IProductsReadStore
{
    private readonly MongoClient _mongoClient;
    private const string CollectionName = "Products";
    private readonly IMongoDatabase _mongoDatabase;

    public ProductsReadStore(IMongoDbConnectionProvider mongoDbConnectionProvider,
        IOptions<DatabaseConfiguration> databaseConfiguration)
    {
        _mongoClient = new MongoClient(mongoDbConnectionProvider.GetMongoUrl());
        _mongoDatabase = _mongoClient.GetDatabase(databaseConfiguration.Value.DatabaseName);
    }

    public async Task<FullProductResponse> GetFullProduct(int id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoCollection<FullProductResponseEntity>
            collection = _mongoDatabase.GetCollection<FullProductResponseEntity>(CollectionName);
        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);
        FullProductResponseEntity entity = await collection.Find(filter).SingleOrDefaultAsync(cancellationToken);
        return entity.ToFullProductResponse();
    }

    public async Task<bool> UpsertProductViewDetails(int id, ProductDetails details,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoCollection<FullProductResponseEntity>
            collection = _mongoDatabase.GetCollection<FullProductResponseEntity>(CollectionName);

        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);

        FullProductResponseEntity entity =
            await collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            ?? new FullProductResponseEntity();

        entity.Id ??= id;
        entity.Details = details;
        entity.Stock = 0; //default
        entity.Price = 0; //Default

        var replaceOne = await collection.ReplaceOneAsync(filter,
            entity,
            new ReplaceOptions()
            {
                IsUpsert = true
            }, cancellationToken);

        return replaceOne.IsAcknowledged;
    }

    public async Task<bool> UpdateProductStock(int id, int stock,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoCollection<FullProductResponseEntity>
            collection = _mongoDatabase.GetCollection<FullProductResponseEntity>(CollectionName);

        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);

        FullProductResponseEntity entity =
            await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);


        entity.Stock = stock;
        var replaceOne = await collection.ReplaceOneAsync(filter,
            entity,
            new ReplaceOptions()
            {
            }, cancellationToken);

        return replaceOne.IsAcknowledged;
    }

    public async Task<bool> UpdateProductPrice(int id, decimal price,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoCollection<FullProductResponseEntity>
            collection = _mongoDatabase.GetCollection<FullProductResponseEntity>(CollectionName);

        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);

        FullProductResponseEntity entity =
                await collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            ;


        entity.Price = (double)price;
        var replaceOne = await collection.ReplaceOneAsync(filter,
            entity,
            new ReplaceOptions()
            {
            }, cancellationToken);

        return replaceOne.IsAcknowledged;
    }


    private class FullProductResponseEntity
    {
        [BsonId] public ObjectId _id { get; set; }
        public int? Id { get; set; }
        public ProductDetails? Details { get; set; }
        public int Stock { get; set; }
        public double Price { get; set; }

        public FullProductResponseEntity()
        {
            _id = ObjectId.GenerateNewId();
        }

        public FullProductResponse ToFullProductResponse()
        {
            return new FullProductResponse((int)Id!, Details!, Stock, (decimal)Price);
        }
    }
}