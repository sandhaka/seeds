namespace Ddd.Domain;

public abstract class DomainEvent
{
    /// <summary>
    /// Event id
    /// </summary>
    public Guid Id { get; }
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Created { get; }

    protected DomainEvent(Guid id, DateTime created)
    {
        Created = created;
        Id = id;
    }
}