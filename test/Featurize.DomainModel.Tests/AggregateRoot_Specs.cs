using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Featurize.DomainModel.Tests.EventCollection_Specs.Version;

namespace Featurize.DomainModel.Tests;

public sealed class AggregateRoot_Specs
{
    [Test]
    public void should_be_able_to_create()
    {
        var aggregate = TestAggregate.Create(Guid.NewGuid());

        aggregate.Version.Should().Be(0);
        aggregate.ExpectedVersion.Should().Be(1);
        aggregate.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(50));
        aggregate.LastModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(50));

        aggregate.GetUncommittedEvents().Should().HaveCount(1)
            .And.AllBeOfType<TestEvent>();
    }

    public class Id
    {
        [Test]
        public void should_be_same_type()
        {
            var guid = Guid.NewGuid();

            var aggregate = TestAggregate.Create(guid);

            aggregate.Id.Should().Be(guid);
        }

        [Test]
        public void should_be_same_from_eventCollection()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = TestAggregate.Create(Guid.NewGuid());

            var events = EventCollection<Guid>.Create(aggregateId, new[]
            {
                new TestEvent(),
            });
            aggregate.LoadFromHistory(events);

            aggregate.Id.Should().Be(aggregateId);
        }


    }

    public class Version
    {
        [Test]
        public void should_be_zero_on_new()
        {
            var guid = Guid.NewGuid();

            var aggregate = TestAggregate.Create(guid);

            aggregate.Version.Should().Be(0);
        }

        [Test]
        public void should_be_same_as_latest_event()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = TestAggregate.Create(aggregateId);
            var events = EventCollection<Guid>.Create(aggregateId, new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
            });
            aggregate.LoadFromHistory(events);

            aggregate.Version.Should().Be(3);
        }
    }

    public class ExpectedVersion
    {
        [Test]
        public void should_be_same_as_version_when_no_new_events()
        {
            var aggregateId = Guid.NewGuid();
            var events = EventCollection<Guid>.Create(aggregateId, new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
            });

            var aggregate = TestAggregate.Create(aggregateId);
            aggregate.LoadFromHistory(events);

            aggregate.ExpectedVersion.Should().Be(aggregate.Version);

        }

        [Test]
        public void should_increase_on_new_event()
        {
            var aggregateId = Guid.NewGuid();
            var events = EventCollection<Guid>.Create(aggregateId, new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
            });

            var aggregate = TestAggregate.Create(aggregateId);
            aggregate.LoadFromHistory(events);

            var newEvents = Random.Shared.Next(1, 9);

            for (int i = 0; i < newEvents; i++)
            {
                aggregate.RecordEvent(new TestEvent());
            }

            aggregate.ExpectedVersion.Should().Be(aggregate.Version + newEvents);

        }
    }

    public class LoadFromHistory
    {
        [Test]
        public void should_apply_events()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = TestAggregate.Create(aggregateId);
            var events = EventCollection<Guid>.Create(aggregateId, new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
            });
            aggregate.LoadFromHistory(events);

            aggregate.ApplyCalled.Should().BeTrue();
            aggregate.ApplyCalledTimes.Should().Be(events.Version + 1);
        }
    }

    public class ApplyEvent
    {
        [Test]
        public void should_call_apply()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = TestAggregate.Create(aggregateId);

            aggregate.ApplyCalled.Should().BeTrue();
            aggregate.ApplyCalledTimes.Should().Be(1);
        }

        [Test]
        public void event_without_handler_should_not_crash()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = TestAggregate.Create(aggregateId);

            var check = () => aggregate.RecordEvent(new TestEventWithoutApply());

            check.Should().NotThrow<StackOverflowException>();
        }
    }
}
