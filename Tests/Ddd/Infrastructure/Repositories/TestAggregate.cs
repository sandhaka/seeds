using System.Text.Json;
using Ddd.Domain.EventSourcing;

namespace Tests.Ddd.Infrastructure.Repositories
{
    public class TestAggregate : EventSourcedAggregate
    {
        public TestAggregate(Guid guid) : base(guid)
        {
        }

        public int Accumulator { get; private set; }

        protected override string SerializeAggregateData() => JsonSerializer.Serialize(new {Accumulator});

        protected override void LoadDataFromSnapshot(string data)
        {
            var deserialized = JsonSerializer.Deserialize<dynamic>(data);
            Accumulator = deserialized!.GetProperty("Accumulator").GetInt32();
        }

        protected override void When(ChangeEvent @event)
        {
            switch (@event)
            {
                case TestEvent evt:
                {
                    Accumulator++;
                    break;
                }
            }
        }

        public void Increment()
        {
            Causes(new TestEvent(Guid.NewGuid(), DateTime.Now));
        }
    }
}