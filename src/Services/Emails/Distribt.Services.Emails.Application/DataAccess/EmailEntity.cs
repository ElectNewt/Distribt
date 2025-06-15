using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Distribt.Services.Emails.Application.DataAccess;

internal class EmailEntity
{
    [BsonId]
    public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
    public string Id { get; set; } = default!;
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    [BsonRepresentation(BsonType.String)]
    public EmailStatus Status { get; set; } = EmailStatus.Received;
}

public enum EmailStatus
{
    Received,
    Sent
}
