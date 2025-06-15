using Distribt.Services.Emails.Controllers;
using Distribt.Shared.Databases.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Distribt.Services.Emails.Application.DataAccess;

public interface IEmailRepository
{
    Task<string> AddEmail(EmailDto email, CancellationToken cancellationToken = default);
    Task MarkAsSent(string id, CancellationToken cancellationToken = default);
}

public class EmailRepository : IEmailRepository
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly EmailRepositoryConfiguration _configuration;

    public EmailRepository(IMongoDbConnectionProvider mongoDbConnectionProvider,
        IOptions<DatabaseConfiguration> databaseConfiguration,
        IOptions<EmailRepositoryConfiguration> repositoryConfiguration)
    {
        _client = new MongoClient(mongoDbConnectionProvider.GetMongoUrl());
        _database = _client.GetDatabase(databaseConfiguration.Value.DatabaseName);
        _configuration = repositoryConfiguration.Value;
    }

    public async Task<string> AddEmail(EmailDto email, CancellationToken cancellationToken = default)
    {
        IMongoCollection<EmailEntity> collection = _database.GetCollection<EmailEntity>(_configuration.CollectionName);
        EmailEntity entity = new EmailEntity
        {
            Id = Guid.NewGuid().ToString(),
            From = email.from,
            To = email.to,
            Subject = email.subject,
            Body = email.body,
            Status = EmailStatus.Received
        };
        await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity.Id;
    }

    public async Task MarkAsSent(string id, CancellationToken cancellationToken = default)
    {
        IMongoCollection<EmailEntity> collection = _database.GetCollection<EmailEntity>(_configuration.CollectionName);
        FilterDefinition<EmailEntity> filter = Builders<EmailEntity>.Filter.Eq("Id", id);
        UpdateDefinition<EmailEntity> update = Builders<EmailEntity>.Update.Set(e => e.Status, EmailStatus.Sent);
        await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

}
