using Distribt.Shared.EventSourcing.Extensions;

namespace Distribt.Shared.EventSourcing;


public class Aggregate
{
    private List<AggregateChange> _changes = new List<AggregateChange>();
    public Guid Id { get; internal set; }

    private string AggregateType => GetType().Name;
    public int Version { get; set; } = 0;
    
    /// <summary>
    /// this flag is used to identify when an event is being loaded from the DB
    /// or when the event is being created as new
    /// </summary>
    private bool ReadingFromHistory { get; set; } = false;

    protected Aggregate(Guid id)
    {
        Id = id;
    }

    internal void Initialize(Guid id)
    {
        Id = id;
        _changes = new List<AggregateChange>();
    }

    public List<AggregateChange> GetUncommittedChanges()
    {
        return _changes.Where(a=>a.IsNew).ToList();
    }

    public void MarkChangesAsCommitted()
    {
        _changes.Clear();
    }

    protected void ApplyChange<T>(T eventObject)
    {
        if (eventObject == null)
            throw new ArgumentException("you cannot pass a null object into the aggregate");

        Version++;

        AggregateChange change = new AggregateChange(
            eventObject,
            Id,
            eventObject.GetType(),
            $"{Id}:{Version}",
            Version,
            ReadingFromHistory != true
        );
        _changes.Add(change);
       
    }


    public void LoadFromHistory(IList<AggregateChange> history)
    {
        if (!history.Any())
        {
            return;
        }

        ReadingFromHistory = true;
        foreach (var e in history)
        {
            ApplyChanges(e.Content);
        }
        ReadingFromHistory = false;

       Version = history.Last().Version;
        
        void ApplyChanges<T>(T eventObject)
        {
            this.AsDynamic()!.Apply(eventObject);
        }
    }
}

