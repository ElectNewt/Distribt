namespace Distribt.Shared.EventSourcing;

public record AggregateChange(object Content, Guid Id, Type Type, string TransactionId, int Version, bool IsNew);

//the dto is the one stored in the DB
public class AggregateChangeDto
{
    public object Content { get; set; }
    public Guid AggregateId { get; set; }

    public string AggregateType { get; set; }
    public string TransactionId { get; set; }
    public int AggregateVersion { get; set; }

    public AggregateChangeDto(object content, Guid aggregateId, string aggregateType, string transactionId, int aggregateVersion)
    {
        Content = content;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        TransactionId = transactionId;
        AggregateVersion = aggregateVersion;
    }
}