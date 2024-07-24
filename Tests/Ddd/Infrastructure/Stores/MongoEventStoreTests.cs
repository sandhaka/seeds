using Ddd.Domain.EventSourcing;
using Ddd.Infrastructure.Stores.EventStore;
using MongoDB.Driver;
using Moq;

namespace Tests.Ddd.Infrastructure.Stores;

public class MongoEventStoreTests
{
    private readonly Mock<IMongoClient> _mongoClient;
    private readonly Mock<IMongoDatabase> _mongoDatabase;
    private readonly Mock<IClientSessionHandle> _sessionHandle;
    private readonly Mock<IMongoCollection<StoredDomainEvent>> _storedEventMongoCollection;
    private readonly Mock<IMongoIndexManager<StoredDomainEvent>> _storedEventMongoIndexManager;
    private readonly Mock<IMongoCollection<EventSourcedAggregateSnapshot>> _eventSourcedAggregateSnapshotMongoCollection;
    private readonly Mock<IMongoIndexManager<EventSourcedAggregateSnapshot>> _eventSourcedAggregateSnapshotIndexManager;
    private MongoEventStore _sut;

    public MongoEventStoreTests()
    {
        _mongoDatabase = new Mock<IMongoDatabase>();
        _sessionHandle = new Mock<IClientSessionHandle>();

        _storedEventMongoCollection = new Mock<IMongoCollection<StoredDomainEvent>>();
        _storedEventMongoIndexManager = new Mock<IMongoIndexManager<StoredDomainEvent>>();

        _eventSourcedAggregateSnapshotMongoCollection = new Mock<IMongoCollection<EventSourcedAggregateSnapshot>>();
        _eventSourcedAggregateSnapshotIndexManager = new Mock<IMongoIndexManager<EventSourcedAggregateSnapshot>>();

        _mongoClient = new Mock<IMongoClient>();

        MainSetup();
    }

    private void MainSetup()
    {
        _mongoClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null))
            .Returns(_mongoDatabase.Object);
        _mongoClient.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>(), default(CancellationToken)))
            .Returns(_sessionHandle.Object);

        _mongoDatabase.Setup(d => d.GetCollection<StoredDomainEvent>(It.IsAny<string>(), null))
            .Returns(_storedEventMongoCollection.Object);
        _mongoDatabase.Setup(d => d.GetCollection<EventSourcedAggregateSnapshot>(It.IsAny<string>(), null))
            .Returns(_eventSourcedAggregateSnapshotMongoCollection.Object);

        _sut = new MongoEventStore(_mongoClient.Object);
    }
        
    [Fact]
    public void ShouldCreateStream()
    {
        // Setup
        _mongoDatabase.Setup(d => d.CreateCollection(It.IsAny<IClientSessionHandle>(), It.IsAny<string>(), null,
            default(CancellationToken)));

        _storedEventMongoCollection.Setup(c => c.Indexes)
            .Returns(_storedEventMongoIndexManager.Object);
        _eventSourcedAggregateSnapshotMongoCollection.Setup(c => c.Indexes)
            .Returns(_eventSourcedAggregateSnapshotIndexManager.Object);

        // Act
        _sut.CreateNewStream("test-stream");

        // Verify collections has been created
        _mongoDatabase.Verify(d => d.CreateCollection(
            It.IsAny<IClientSessionHandle>(),
            It.Is<string>(s => s.Equals("test-stream")), null,
            default(CancellationToken)), Times.Exactly(1));
        _mongoDatabase.Verify(d => d.CreateCollection(
            It.IsAny<IClientSessionHandle>(),
            It.Is<string>(s => s.Equals("test-stream_snapshots")), null,
            default(CancellationToken)), Times.Exactly(1));

        // Verify collections has been retrieved
        _mongoDatabase.Verify(d =>
                d.GetCollection<StoredDomainEvent>(It.Is<string>(s => s.Equals("test-stream")),
                    null),
            Times.Exactly(1));
        _mongoDatabase.Verify(d =>
                d.GetCollection<EventSourcedAggregateSnapshot>(It.Is<string>(s => s.Equals("test-stream_snapshots")),
                    null),
            Times.Exactly(1));

        // Verify indexes has been created
        _storedEventMongoIndexManager.Verify(m =>
            m.CreateOne(It.IsAny<IClientSessionHandle>(),
                It.IsAny<CreateIndexModel<StoredDomainEvent>>(),
                null,
                default(CancellationToken)), Times.Exactly(1));
        _eventSourcedAggregateSnapshotIndexManager.Verify(m =>
            m.CreateOne(It.IsAny<IClientSessionHandle>(),
                It.IsAny<CreateIndexModel<EventSourcedAggregateSnapshot>>(),
                null,
                default(CancellationToken)), Times.Exactly(1));
    }

    [Fact]
    public void ShouldAppendEventsToStream()
    {
        // Setup
        _storedEventMongoCollection.Setup(c => c.InsertMany(It.IsAny<IClientSessionHandle>(),
            It.IsAny<IEnumerable<StoredDomainEvent>>(), null, default(CancellationToken)));

        // Act
        _sut.AppendEventsToStream("test-stream", new List<ChangeEvent>
        {
            new TestEvent(Guid.NewGuid(), DateTime.Now),
            new TestEvent(Guid.NewGuid(), DateTime.Now)
        }, 0);

        // Verify
        _mongoDatabase.Verify(d =>
                d.GetCollection<StoredDomainEvent>(It.Is<string>(s => s.Equals("test-stream")),
                    null),
            Times.Exactly(1));
        _storedEventMongoCollection.Verify(c =>
            c.InsertMany(
                It.IsAny<IClientSessionHandle>(),
                It.Is<IEnumerable<StoredDomainEvent>>(array =>
                    // ReSharper disable once PossibleMultipleEnumeration
                    array.Count() == 2 &&
                    // ReSharper disable once PossibleMultipleEnumeration
                    array.All(e => GuidVerify(e.Id))),
                null,
                default(CancellationToken)), Times.Exactly(1));
    }

    private static bool GuidVerify(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}

internal class TestEvent : ChangeEvent
{
    public TestEvent(Guid id, DateTime created) : base(id, created)
    {

    }
}