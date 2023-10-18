using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Featurize.DomainModel.Tests")]

namespace Featurize.DomainModel;

/// <summary>
/// 
/// </summary>
public static class AggregateRoot
{
    /// <summary>
    /// Creates a AggregateRoot.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Throw an exception if the method is not implemented.</exception>
    public static TAggregate Create<TAggregate, TId>(TId aggregateId)
        where TAggregate : AggregateRoot<TAggregate, TId>
        where TId: struct
    {
        var methodName = "Create";
        var type = typeof(TAggregate);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, new[] { typeof(TId) });

        if(method == null)
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, new[] { typeof(TId) });
            var result = constructor?.Invoke(new object[] { aggregateId });
            if (result is TAggregate returnValue) return returnValue;
        } 
        else 
        { 
            var result = method.Invoke(null, new object[] { aggregateId });
            if(result is TAggregate returnValue) return returnValue;
        }
        
        throw new NotImplementedException($"The aggregate does not have a public static method 'public static {type.Name} {methodName}({typeof(TId).Name} id)'.");
    }
}

/// <summary>
/// Base class for an AggregateRoot.
/// </summary>
/// <typeparam name="TSelf">The type of the aggregate.</typeparam>
/// <typeparam name="TId">The type of the Identifier of the aggregate.</typeparam>
public abstract class AggregateRoot<TSelf, TId>
    where TId : struct
{
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


    /// <summary>
    /// Adds and applies an event to the aggregate.
    /// </summary>
    /// <param name="e"></param>
    protected internal void RecordEvent(EventRecord e)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        ArgumentNullException.ThrowIfNull(Id, nameof(Id));

        RecordEvent(e, true);
    }

    private void RecordEvent(EventRecord e, bool isNew)
    {
        ApplyInternal(e);
        if (isNew)
        {
            _events.Append(e);
        }
    }

    private const string ApplyMehodName = "Apply";
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
