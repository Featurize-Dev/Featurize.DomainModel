namespace Featurize.DomainModel;

/// <summary>
/// A Append only EventCollection
/// </summary>
/// <typeparam name="TId">The Type of the aggregateId</typeparam>
public class EventCollection<TId> : IReadOnlyCollection<EventRecord>
    where TId : struct
{
    private EventRecord[] _events = Array.Empty<EventRecord>();
    private readonly List<EventRecord> _uncomitted = new();

    /// <summary>
    /// The Id of the aggregate
    /// </summary>
    public TId AggregateId { get; }
    /// <summary>
    /// The version of the aggregate
    /// </summary>
    public int Version
        => _events.Length;
    /// <summary>
    /// The expected version after save.
    /// </summary>
    public int ExpectedVersion
        => Version + _uncomitted.Count;

    /// <summary>
    /// The creation date of this event stream.
    /// </summary>
    public DateTimeOffset? CreatedOn
        => _events.Any()
        ? _events.FirstOrDefault()?.OccouredOn
        : _uncomitted.FirstOrDefault()?.OccouredOn;

    /// <summary>
    /// The date of the last modification.
    /// </summary>
    public DateTimeOffset? LastModifiedOn
        => _uncomitted.Any()
        ? _uncomitted.LastOrDefault()?.OccouredOn
        : _events.LastOrDefault()?.OccouredOn;

    /// <summary>
    /// The total number event comitted.
    /// </summary>
    public int Count
        => _events.Length;

    /// <summary>
    /// Creates a new EventCollection for an AggregateId
    /// </summary>
    /// <param name="aggregateId"></param>
    private EventCollection(TId aggregateId)
    {
        AggregateId = aggregateId;
    }

    /// <summary>
    /// Creates an instance of an EventCollection
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate</param>
    /// <param name="events">Array of events</param>
    /// <returns>Returns an event collection </returns>
    public static EventCollection<TId> Create(TId aggregateId, EventRecord[]? events = null)
        => new(aggregateId)
        {
            _events = events ?? Array.Empty<EventRecord>(),
        };

    /// <summary>
    /// Append a event to the collection.
    /// </summary>
    /// <param name="e"></param>
    public void Append(EventRecord e)
    {
        _uncomitted.Add(e);
    }

    /// <summary>
    /// Get the uncommitted events
    /// </summary>
    /// <returns>Events that are uncommitted.</returns>
    public EventCollection<TId> GetUncommittedEvents()
        => Create(AggregateId, _uncomitted.ToArray());

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    public IEnumerator<EventRecord> GetEnumerator()
        => _events.ToList().GetEnumerator();

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
