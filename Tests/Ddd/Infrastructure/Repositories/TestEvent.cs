using Ddd.Domain.EventSourcing;

namespace Tests.Ddd.Infrastructure.Repositories;

public class TestEvent : ChangeEvent
{
    public TestEvent(Guid id, DateTime created) : base(id, created)
    {

    }
}