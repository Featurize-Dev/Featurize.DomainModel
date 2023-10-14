using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Featurize.DomainModel.Tests")]

namespace Featurize.DomainModel;

/// <summary>
/// Base class for an AggregateRoot.
/// </summary>
/// <typeparam name="TSelf">The type of the aggregate.</typeparam>
/// <typeparam name="TId">The type of the Identifier of the aggregate.</typeparam>
public abstract class AggregateRoot<TSelf, TId>
    where TId : struct
{
    private const string ApplyMehodName = "Apply";

    private EventCollection<TId> _events;

    /// <summary>
    /// The identifier of this aggregate.
    /// </summary>
    public TId Id => _events.AggregateId;

    /// <summary>
    /// The version of the aggregate root.
    /// </summary>
    public int Version => _events.Version;

    /// <summary>
    /// The Expected verion after saving the aggregate.
    /// </summary>
    public int ExpectedVersion => _events.ExpectedVersion;

    /// <summary>
    /// The date the aggregate was created.
    /// </summary>
    public DateTimeOffset? CreatedOn => _events.CreatedOn;

    /// <summary>
    /// The Date the aggregate was last modified.
    /// </summary>
    public DateTimeOffset? LastModifiedOn => _events.LastModifiedOn;

    /// <summary>
    /// Constructor to instanciate a new instance of <see cref="TSelf" />.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected AggregateRoot(TId id)
    {
        _events = EventCollection<TId>.Create(id);
    }

    /// <summary>
    /// Adds and applies an event to the aggregate.
    /// </summary>
    /// <param name="e"></param>
    internal void RecordEvent(EventRecord e)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        ArgumentNullException.ThrowIfNull(Id, nameof(Id));

        RecordEvent(e, true);
    }

    /// <summary>
    /// Refresh aggregate from an event stream. 
    /// </summary>
    /// <param name="events">a event collection.</param>
    public void LoadFromHistory(EventCollection<TId> events)
    {
        _events = events;
        foreach (var e in _events)
        {
            RecordEvent(e, false);
        }
    }

    /// <summary>
    /// Gets events that are not committed.
    /// </summary>
    /// <returns>a event collection.</returns>
    public EventCollection<TId> GetUncommittedEvents()
        => _events.GetUncommittedEvents();

    private void RecordEvent(EventRecord e, bool isNew)
    {
        ApplyInternal(e);
        if (isNew)
        {
            _events.Append(e);
        }
    }

    private void ApplyInternal(EventRecord e)
    {
        SafeInvokeMethod(GetType(), this, ApplyMehodName, e);
    }

    private void SafeInvokeMethod(Type type, object target, string name, params object[] args)
    {
        const BindingFlags privateOrPublicMethodFlags = BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        try
        {
            type.InvokeMember(name, privateOrPublicMethodFlags, null, target, args);
        }
        catch (MissingMethodException)
        {
            if (type.BaseType != null)
            {
                SafeInvokeMethod(type.BaseType, target, name, args);
            }
        }
    }
}
