using System.Text.Json;

namespace Ddd.Domain.EventSourcing;

/// <summary>
/// Domain event base class
/// </summary>
public abstract class ChangeEvent
{
    /// <summary>
    /// Event id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Created { get; set; }

    protected ChangeEvent(Guid id, DateTime created)
    {
        Created = created;
        Id = id;
    }

    /// <summary>
    /// Convert a domain event in a storable version
    /// </summary>
    /// <returns>Storable domain event: <see cref="StoredDomainEvent"/></returns>
    public StoredDomainEvent ToStorableDto()
    {
        return new StoredDomainEvent
        {
            Type = GetType().AssemblyQualifiedName,
            Created = Created,
            Data = JsonSerializer.Serialize(this)
        };
    }
}