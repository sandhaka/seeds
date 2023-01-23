using System.Text.Json;
using Ddd.Domain.EventSourcing;
using Ddd.Domain.EventSourcing.Exceptions;
using MongoDB.Driver;

namespace Ddd.Infrastructure.Stores.EventStore;

public class MongoEventStore : IEventStore
{
    private readonly IMongoDatabase _database;
    private readonly IClientSessionHandle _sessionHandle;

    public MongoEventStore(IMongoClient mongoClient)
    {
        _database = mongoClient.GetDatabase(
            Environment.GetEnvironmentVariable("MONGODB_EVENTSTORE_DB_NAME") ?? "event_store");

        _sessionHandle = mongoClient.StartSession(
            new ClientSessionOptions
            {
                CausalConsistency = true,
                DefaultTransactionOptions = new TransactionOptions(
                    ReadConcern.Majority,
                    ReadPreference.Primary,
                    WriteConcern.WMajority)
            });
    }

    #region [ Events Stream ]

    /// <summary>
    /// Create a new events stream
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <returns></returns>
    /// <exception cref="StreamEmptyException">Stream is empty</exception>
    public void CreateNewStream(string streamName)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        CreateStream(streamName);
    }

    /// <summary>
    /// Append streams
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <param name="domainEvents">A collection of events</param>
    /// <param name="expectedVersion">Expected aggregate version to ensure optimistic concurrency</param>
    /// <exception cref="Exception">Stream is empty</exception>
    public void AppendEventsToStream(string streamName,
        IEnumerable<ChangeEvent> domainEvents, int expectedVersion)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }
        var events = domainEvents as ChangeEvent[] ?? domainEvents.ToArray();
        if (!events.Any())
        {
            throw new StreamEmptyException(streamName);
        }

        InternalAppendEventsToStream(streamName, events, expectedVersion);
    }

    /// <summary>
    /// Get a stream of events
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <param name="fromVersion">From version</param>
    /// <param name="toVersion">To version</param>
    /// <returns>A collection of domain events</returns>
    public IEnumerable<ChangeEvent> GetStream(string streamName, int fromVersion, int toVersion)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        var collection = GetDomainEventsCollection(streamName);

        var stream = collection.Find(FilterDefinition<StoredDomainEvent>.Empty)
            .Skip(fromVersion)
            .Limit(toVersion - fromVersion)
            .ToList();

        return stream.Select(evt => 
            (ChangeEvent) JsonSerializer.Deserialize(evt.Data, Type.GetType(evt.Type)!)!);
    }

    /// <summary>
    /// Retrieve an aggregate version by specific date and time
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <param name="at">DateTime filter</param>
    /// <returns>Version</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int? GetVersionAt(string streamName, DateTime at)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        return GetDomainEventsCollection(streamName)
            .Find(f => f.Created <= at)
            .Sort("{Version:-1}")
            .FirstOrDefault()?.Version;
    }

    /// <summary>
    /// Return stream size
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <returns>Stream size</returns>
    public long GetEventsStreamSize(string streamName)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        return GetDomainEventsCollection(streamName)
            .CountDocuments(FilterDefinition<StoredDomainEvent>.Empty);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stramName"></param>
    /// <returns></returns>
    public bool Exists(string stramName)
    {
        var collections = _database.ListCollectionNames();
        while ((collections.MoveNext()))
        {
            var collection = collections.Current;
            if (collection.Any(name => name.Equals(stramName)))
            {
                return true;
            }
        }

        return false;
    }

    private void InternalAppendEventsToStream(string streamName, IEnumerable<ChangeEvent> events,
        int expectedVersion)
    {
        var collection = GetDomainEventsCollection(streamName);

        var enumerable = events.Select(evt => evt.ToStorableDto()).ToList();

        foreach (var evt in enumerable)
        {
            evt.Id = Guid.NewGuid().ToString();
            evt.Version = expectedVersion++;
        }

        // Append the events to the store
        collection.InsertMany(_sessionHandle, enumerable);
    }

    private void CreateStream(string streamName)
    {
        _database.CreateCollection(_sessionHandle, streamName);
        _database.CreateCollection(_sessionHandle,$"{streamName}_snapshots");

        // Ensure Version will be unique to manage optimistic concurrency
        GetDomainEventsCollection(streamName).Indexes.CreateOne(
            _sessionHandle,
            new CreateIndexModel<StoredDomainEvent>(new JsonIndexKeysDefinition<StoredDomainEvent>(
                    "{Version:1}"),
                new CreateIndexOptions { Unique = true }));
        GetAggregateSnapshotCollection(streamName).Indexes.CreateOne(
            _sessionHandle,
            new CreateIndexModel<EventSourcedAggregateSnapshot>(
                new JsonIndexKeysDefinition<EventSourcedAggregateSnapshot>(
                    "{Version:1}"),
                new CreateIndexOptions { Unique = true }));
    }

    private IMongoCollection<StoredDomainEvent> GetDomainEventsCollection(string streamName)
    {
        return _database.GetCollection<StoredDomainEvent>(streamName);
    }

    #endregion

    #region [ Snapshot ]

    /// <summary>
    /// Get the last recent snapshot
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <returns>Snapshot</returns>
    public EventSourcedAggregateSnapshot GetLatestSnapshot(string streamName)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        var filters = FilterDefinition<EventSourcedAggregateSnapshot>.Empty;

        return GetAggregateSnapshotCollection(streamName)
            .Find(filters)
            .Sort("{Version:-1}")
            .FirstOrDefault();
    }

    /// <summary>
    /// Get snapshot
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <param name="at">DateTime filter</param>
    /// <returns>Snapshot</returns>
    public EventSourcedAggregateSnapshot GetSnapshot(string streamName, DateTime at)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        return GetAggregateSnapshotCollection(streamName)
            .Find(c => c.Created <= at)
            .Sort("{Version:-1}")
            .FirstOrDefault();
    }

    /// <summary>
    /// Add aggregate snapshot
    /// </summary>
    /// <param name="streamName">Stream name</param>
    /// <param name="snapshot">Snapshot</param>
    public void AddSnapshot(string streamName, EventSourcedAggregateSnapshot snapshot)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }
        if (!Guid.TryParse(snapshot.Id, out _) || Type.GetType(snapshot.Type) == null)
        {
            throw new ArgumentException("Invalid aggregate");
        }

        InternalAddSnapshot(streamName, snapshot);
    }

    private void InternalAddSnapshot(string streamName, EventSourcedAggregateSnapshot snapshot)
    {
        GetAggregateSnapshotCollection(streamName)
            .InsertOne(_sessionHandle, snapshot);
    }

    private IMongoCollection<EventSourcedAggregateSnapshot> GetAggregateSnapshotCollection(string streamName)
    {
        return _database.GetCollection<EventSourcedAggregateSnapshot>($"{streamName}_snapshots");
    }

    #endregion

    #region [ Transaction support ]

    /// <summary>
    /// Start a new transaction
    /// </summary>
    /// <returns>Task</returns>
    public void StarTransaction()
    {
        if (_sessionHandle.IsInTransaction)
        {
            throw new InvalidOperationException(
                "Events store has a pending transaction. And the current transaction by " +
                "EndActiveTransactionAndCommitAsync");
        }

        _sessionHandle.StartTransaction();
    }

    /// <summary>
    /// End transaction and commit changes
    /// </summary>
    /// <returns>Task</returns>
    public async Task EndActiveTransactionAndCommitAsync()
    {
        await ExecuteEventStoreOperationWithWriteRetries(async () =>
        {
            await _sessionHandle.CommitTransactionAsync();
        });
    }

    /// <summary>
    /// Abort current transaction
    /// </summary>
    /// <returns>Task</returns>
    public async Task AbortActiveTransactionAsync()
    {
        await _sessionHandle.AbortTransactionAsync();
    }

    private async Task ExecuteEventStoreOperationWithWriteRetries(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (MongoException e)
        {
            if (!e.HasErrorLabel("TransientTransactionError"))
            {
                throw;
            }

            await ExecuteEventStoreOperationWithWriteRetries(action);
        }
    }

    #endregion
}