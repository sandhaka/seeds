using Ddd.Domain.EventSourcing;

namespace Ddd.Infrastructure.Stores.EventStore;

public interface IEventStore
{
    // Event Stream
    void CreateNewStream(string streamName);
    void AppendEventsToStream(string streamName, IEnumerable<ChangeEvent> domainEvents, int expectedVersion);
    long GetEventsStreamSize(string streamName);
    IEnumerable<ChangeEvent> GetStream(string streamName, int fromVersion, int toVersion);
    int? GetVersionAt(string streamName, DateTime at);
    bool Exists(string stramName);

    // Snapshot
    void AddSnapshot(string streamName, EventSourcedAggregateSnapshot snapshot);
    EventSourcedAggregateSnapshot? GetLatestSnapshot(string streamName);
    EventSourcedAggregateSnapshot? GetSnapshot(string streamName, DateTime at);

    // Transaction support
    void StarTransaction();
    Task EndActiveTransactionAndCommitAsync();
    Task AbortActiveTransactionAsync();
}