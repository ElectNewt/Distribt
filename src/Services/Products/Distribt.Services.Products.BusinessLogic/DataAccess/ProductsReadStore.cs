using Distribt.Services.Products.Dtos;
using Distribt.Shared.Databases.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Distribt.Services.Products.BusinessLogic.DataAccess;

public interface IProductsReadStore
{
    Task<FullProductResponse> GetFullProduct(int id, CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> UpsertProductViewDetails(int id, ProductDetails details, CancellationToken cancellationToken = default(CancellationToken));
}

public class ProductsReadStore : IProductsReadStore
{
    //TODO: #20
    private readonly MongoClient _mongoClient;

    public ProductsReadStore(IMongoDbConnectionProvider mongoDbConnectionProvider)
    {
        _mongoClient = new MongoClient(mongoDbConnectionProvider.GetMongoUrl());
    }

    public async Task<FullProductResponse> GetFullProduct(int id, CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoDatabase db = _mongoClient.GetDatabase("distribt");
        IMongoCollection<FullProductResponseEntity>
            collection = db.GetCollection<FullProductResponseEntity>("Products");
        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);
        FullProductResponseEntity entity = await collection.Find(filter).SingleOrDefaultAsync(cancellationToken);
        return entity.ToFullProductResponse();
    }

    public async Task<bool> UpsertProductViewDetails(int id, ProductDetails details, CancellationToken cancellationToken = default(CancellationToken))
    {
        IMongoDatabase db = _mongoClient.GetDatabase("distribt");
        IMongoCollection<FullProductResponseEntity>
            collection = db.GetCollection<FullProductResponseEntity>("Products");

        FilterDefinition<FullProductResponseEntity> filter = Builders<FullProductResponseEntity>.Filter.Eq("Id", id);

        FullProductResponseEntity entity =
            await collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            ?? new FullProductResponseEntity();
        
        entity.Id ??= id;
        entity.Details = details;

        var replaceOne = await collection.ReplaceOneAsync(filter,
            entity,
            new ReplaceOptions()
            {
                IsUpsert = true
            }, cancellationToken);

        return replaceOne.IsAcknowledged;
    }


    private class FullProductResponseEntity
    {
        [BsonId] public ObjectId _Id { get; set; }
        public int? Id { get; set; }
        public ProductDetails? Details { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }

        public FullProductResponse ToFullProductResponse()
        {
            return new FullProductResponse((int)Id!, Details!, Stock, Price);
        }
    }
}