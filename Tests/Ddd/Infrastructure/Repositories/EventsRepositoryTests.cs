using System.Collections.Immutable;
using System.Text.Json;
using Ddd.Domain.EventSourcing;
using Ddd.Domain.EventSourcing.Exceptions;
using Ddd.Infrastructure.Repositories;
using Ddd.Infrastructure.Stores.EventStore;
using Moq;

namespace Tests.Ddd.Infrastructure.Repositories;

public class EventsRepositoryTests
{
    private readonly Mock<IEventStore> _eventStore;
    private readonly EventSourcedRepository<TestAggregate> _sut;

    public EventsRepositoryTests()
    {
        _eventStore = new Mock<IEventStore>();
        _sut = new EventSourcedRepository<TestAggregate>(_eventStore.Object);
    }

    #region Find

    [Fact]
    public void ShouldReturnEsByIdWithNoSnapshot()
    {
        // Setup
        var guid = Guid.NewGuid();
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<ChangeEvent>
            {
                new TestEvent(Guid.NewGuid(), DateTime.Now),
                new TestEvent(Guid.NewGuid(), DateTime.Now),
                new TestEvent(Guid.NewGuid(), DateTime.Now),
                new TestEvent(Guid.NewGuid(), DateTime.Now)
            });

        // Act
        var builtAggregate = _sut.FindById(guid);

        // Verify
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        Assert.IsType<TestAggregate>(builtAggregate);
        Assert.True(builtAggregate.Version.Equals(4));
        Assert.True(builtAggregate.Accumulator.Equals(4));
    }

    [Fact]
    public void ShouldReturnEsByIdWithSnapshot()
    {
        // Setup
        var guid = Guid.NewGuid();
        var Accumulator = 4;
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns(new EventSourcedAggregateSnapshot
            {
                Created = DateTime.Parse("10/01/2019 22:23:13"),
                Data = JsonSerializer.Serialize(new { Accumulator }),
                Version = 4,
                Type = "DomainSeeds.lib.Tests.EventSourcing.TestAggregate, DomainSeeds.lib.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            });
        _eventStore.Setup(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<ChangeEvent>
            {
                new TestEvent(Guid.NewGuid(), DateTime.Now),
            }.ToImmutableList());

        // Act
        var builtAggregate = _sut.FindById(guid);
        builtAggregate!.Increment();

        // Verify
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        Assert.IsType<TestAggregate>(builtAggregate);
        Assert.True(builtAggregate.Accumulator.Equals(6));
        Assert.True(builtAggregate.Version.Equals(6));
        Assert.True(builtAggregate.Changes.Count == 1);
    }

    [Fact]
    public void ShouldReturnEsByIdWithNoSnapshotFromAHugeEventsList()
    {
        // Setup
        var eventsList = new List<ChangeEvent>(new int[4096].Select(i => new TestEvent(Guid.NewGuid(), DateTime.Now)));

        var guid = Guid.NewGuid();
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(eventsList);

        // Act
        var builtAggregate = _sut.FindById(guid);

        // Verify
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        Assert.IsType<TestAggregate>(builtAggregate);
        Assert.True(builtAggregate.Version.Equals(4096));
        Assert.True(builtAggregate.Accumulator.Equals(4096));
    }

    [Fact]
    public void ShouldReturnEsByIdWithDateTimeAndNoSnapshot()
    {
        // Setup
        var guid = Guid.NewGuid();
        _eventStore.Setup(ev => ev.GetSnapshot(It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.GetVersionAt(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(3);
        _eventStore.Setup(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), 3))
            .Returns(new List<ChangeEvent>
            {
                new TestEvent(Guid.NewGuid(), DateTime.Now),
                new TestEvent(Guid.NewGuid(), DateTime.Now),
                new TestEvent(Guid.NewGuid(), DateTime.Now)
            });

        // Act
        var builtAggregate = _sut.FindByIdAndTime(guid, DateTime.Now);

        // Verify
        _eventStore.Verify(ev => ev.GetSnapshot(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetVersionAt(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(It.IsAny<string>(), It.IsAny<int>(), 3), Times.Exactly(1));
        Assert.IsType<TestAggregate>(builtAggregate);
        Assert.True(builtAggregate.Version.Equals(3));
        Assert.True(builtAggregate.Accumulator.Equals(3));
    }

    #endregion

    #region Add

    [Fact]
    public void ShouldStoreAnAggregateAsAnEventsStream()
    {
        // Setup
        _eventStore.Setup(ev => ev.CreateNewStream(It.IsAny<string>()));
        var aggr = new TestAggregate(Guid.NewGuid());

        // Act
        _sut.Add(aggr);   
            
        // Verify
        _eventStore.Verify(ev => ev.CreateNewStream(It.IsAny<string>()), Times.Exactly(1));
    }

    [Fact]
    public void ShouldThrowArgumentExceptionOnStoreAnAggregateAsAnEventsStream()
    {
        // Setup
        _eventStore.Setup(ev => ev.CreateNewStream(It.IsAny<string>()));
        var aggr = new TestAggregate(Guid.Empty);

        // Act
        var recordedException = Record.Exception(() =>
        {
            _sut.Add(aggr);
        });

        // Verify
        _eventStore.Verify(ev => ev.CreateNewStream(It.IsAny<string>()), Times.Never);
        Assert.IsType<ArgumentException>(recordedException);
    }

    #endregion

    #region Save

    [Fact]
    public void ShouldSaveTheAggregateStream()
    {
        // Setup
        var aggr = new TestAggregate(Guid.NewGuid());
        aggr.Increment();

        _eventStore.Setup(ev =>
            ev.AppendEventsToStream(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChangeEvent>>(),
                It.IsAny<int>()));
        _eventStore.Setup(ev => ev.GetEventsStreamSize(It.IsAny<string>())).Returns(1);
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.AddSnapshot(It.IsAny<string>(), It.IsAny<EventSourcedAggregateSnapshot>()));

        // Act
        _sut.Save(aggr);

        // Verify
        _eventStore.Verify(ev => ev.AppendEventsToStream(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ChangeEvent>>(),
            It.IsAny<int>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetEventsStreamSize(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.AddSnapshot(
            It.IsAny<string>(),
            It.IsAny<EventSourcedAggregateSnapshot>()), Times.Never);
    }

    [Fact]
    public void ShouldThrowDomainEventsToAddLimitExceptionOnSaveTheAggregateStream()
    {
        // Setup
        var aggr = new TestAggregate(Guid.NewGuid());
        for (var i = 0; i < 129; i++)
        {
            aggr.Increment();
        }

        _eventStore.Setup(ev =>
            ev.AppendEventsToStream(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChangeEvent>>(),
                It.IsAny<int>()));
        _eventStore.Setup(ev => ev.GetEventsStreamSize(It.IsAny<string>())).Returns(1);
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.AddSnapshot(It.IsAny<string>(), It.IsAny<EventSourcedAggregateSnapshot>()));

        // Act
        var recordedException = Record.Exception(() =>
        {
            _sut.Save(aggr);
        });

        // Verify
        _eventStore.Verify(ev => ev.AppendEventsToStream(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ChangeEvent>>(),
            It.IsAny<int>()), Times.Never);
        _eventStore.Verify(ev => ev.GetEventsStreamSize(It.IsAny<string>()), Times.Never);
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Never);
        _eventStore.Verify(ev => ev.AddSnapshot(
            It.IsAny<string>(),
            It.IsAny<EventSourcedAggregateSnapshot>()), Times.Never);
        Assert.IsType<DomainEventsToAddLimitException>(recordedException);
    }

    [Fact]
    public void ShouldDoNothingOnSaveTheAggregateStream()
    {
        // Setup
        var aggr = new TestAggregate(Guid.NewGuid());

        _eventStore.Setup(ev =>
            ev.AppendEventsToStream(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChangeEvent>>(),
                It.IsAny<int>()));
        _eventStore.Setup(ev => ev.GetEventsStreamSize(It.IsAny<string>())).Returns(1);
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.AddSnapshot(It.IsAny<string>(), It.IsAny<EventSourcedAggregateSnapshot>()));

        // Act
        _sut.Save(aggr);

        // Verify
        _eventStore.Verify(ev => ev.AppendEventsToStream(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ChangeEvent>>(),
            It.IsAny<int>()), Times.Never);
        _eventStore.Verify(ev => ev.GetEventsStreamSize(It.IsAny<string>()), Times.Never);
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Never);
        _eventStore.Verify(ev => ev.AddSnapshot(
            It.IsAny<string>(),
            It.IsAny<EventSourcedAggregateSnapshot>()), Times.Never);
    }

    [Fact]
    public void ShouldSaveTheAggregateStreamAndCreateOneSnapshot()
    {
        // Setup
        var aggr = new TestAggregate(Guid.NewGuid());
        aggr.Increment();

        _eventStore.Setup(ev =>
            ev.AppendEventsToStream(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChangeEvent>>(),
                It.IsAny<int>()));
        _eventStore.Setup(ev => ev.GetEventsStreamSize(It.IsAny<string>())).Returns(128);
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns((EventSourcedAggregateSnapshot) null);
        _eventStore.Setup(ev => ev.AddSnapshot(It.IsAny<string>(), It.IsAny<EventSourcedAggregateSnapshot>()));
        var eventsList = new List<ChangeEvent>(new int[128].Select(i => new TestEvent(Guid.NewGuid(), DateTime.Now)));
        _eventStore.Setup(ev => ev.GetStream(
                It.IsAny<string>(),
                It.Is<int>(v => v == 0),
                It.Is<int>(v => v == 128)))
            .Returns(eventsList);

        // Act
        _sut.Save(aggr);

        // Verify
        _eventStore.Verify(ev => ev.AppendEventsToStream(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ChangeEvent>>(),
            It.IsAny<int>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetEventsStreamSize(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(2));
        _eventStore.Verify(ev => ev.AddSnapshot(
            It.IsAny<string>(),
            It.IsAny<EventSourcedAggregateSnapshot>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(
            It.IsAny<string>(),
            It.Is<int>(v => v == 0),
            It.Is<int>(v => v == 128)), Times.Exactly(1));
    }

    [Fact]
    public void ShouldSaveTheAggregateStreamAndCreateOneSnapshotWithAnExistentOne()
    {
        // Setup
        var Accumulator = 128;
        var aggr = new TestAggregate(Guid.NewGuid());
        for (var i = 0; i < 10; i++)
        {
            aggr.Increment();
        }

        _eventStore.Setup(ev =>
            ev.AppendEventsToStream(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChangeEvent>>(),
                It.IsAny<int>()));
        _eventStore.Setup(ev => ev.GetEventsStreamSize(It.IsAny<string>())).Returns(256);
        _eventStore.Setup(ev => ev.GetLatestSnapshot(It.IsAny<string>()))
            .Returns(new EventSourcedAggregateSnapshot
            {
                Created = DateTime.Parse("10/01/2019 22:23:13"),
                Data = JsonSerializer.Serialize(new { Accumulator }),
                Version = 128,
                Type = "DomainSeeds.lib.Tests.EventSourcing.TestAggregate, DomainSeeds.lib.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            });
        _eventStore.Setup(ev => ev.AddSnapshot(It.IsAny<string>(), It.IsAny<EventSourcedAggregateSnapshot>()));
        var eventsList = new List<ChangeEvent>(new int[128].Select(i => new TestEvent(Guid.NewGuid(), DateTime.Now)));
        _eventStore.Setup(ev => ev.GetStream(
                It.IsAny<string>(),
                It.Is<int>(v => v == 128),
                It.Is<int>(v => v == 256)))
            .Returns(eventsList);

        // Act
        _sut.Save(aggr);

        // Verify
        _eventStore.Verify(ev => ev.AppendEventsToStream(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<ChangeEvent>>(),
            It.IsAny<int>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetEventsStreamSize(It.IsAny<string>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetLatestSnapshot(It.IsAny<string>()), Times.Exactly(2));
        _eventStore.Verify(ev => ev.AddSnapshot(
            It.IsAny<string>(),
            It.IsAny<EventSourcedAggregateSnapshot>()), Times.Exactly(1));
        _eventStore.Verify(ev => ev.GetStream(
            It.IsAny<string>(),
            It.Is<int>(v => v == 128),
            It.Is<int>(v => v == 256)), Times.Exactly(1));
    }

    #endregion
}