namespace Featurize.DomainModel;

/// <summary>
/// Base class for events.
/// </summary>
public abstract record EventRecord
{
    /// <summary>
    /// The DateTimeOffset of when this event happend.
    /// </summary>
    public DateTimeOffset OccouredOn { get; set; } = DateTimeOffset.UtcNow;
}
