namespace Ddd.Domain;

public abstract class Aggregate
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// A notifications list directed to other domain aggregates to be
    /// processed in the current transaction
    /// </summary>
    public List<DomainEvent> DomainEvents => _domainEvents;

    public int Version { get; protected set; }

    public void AddDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    public abstract bool IsTransient();
}