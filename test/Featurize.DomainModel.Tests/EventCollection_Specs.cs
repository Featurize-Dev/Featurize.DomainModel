using FluentAssertions;
using static Featurize.DomainModel.Tests.EventCollection_Specs.Version;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Featurize.DomainModel.Tests;

public partial class EventCollection_Specs
{
    public static Random rnd = new();

    public class AggregateId
    {
        public EventCollection<Guid> _collection;
        public Guid _id = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            var version = rnd.Next(100);
            var events = new EventRecord[version];

            for (int i = 0; i < version; i++)
            {
                events[i] = new TestEvent();
            }

            _collection = EventCollection<Guid>.Create(_id, events);

        }

        [Test]
        public void should_be_set_when_new_collection_is_created()
        {
            _collection.AggregateId.Should().Be(_id);
        }
    }

    public partial class Version
    {
        public EventCollection<Guid> _collection;
        public Guid _id = Guid.NewGuid();
        public int _version = rnd.Next(100);

        [SetUp]
        public void Setup()
        {
            var events = new EventRecord[_version];

            for (int i = 0; i < _version; i++)
            {
                events[i] = new TestEvent();
            }

            _collection = EventCollection.Create(_id, events);

        }
        
        [Test]
        public void should_equal_to_the_number_of_events()
        {
            _collection.Version.Should().Be(_version);
        }

        [Test]
        public void should_not_change_if_new_event_is_applied()
        {
            _collection.Append(new TestEvent());
                        
            _collection.Version.Should().Be(_version);
        }
    }

    public class ExpectedVersion
    {
        public EventCollection<Guid> _collection;
        public Guid _id = Guid.NewGuid();
        public int _version = rnd.Next(100);

        [SetUp]
        public void Setup()
        {
            var events = new EventRecord[_version];

            for (int i = 0; i < _version; i++)
            {
                events[i] = new TestEvent();
            }

            _collection = EventCollection.Create(_id, events);

        }

        [Test]
        public void should_be_current_version_plus_count_new_events()
        {
            var newEvents = rnd.Next(50);
            for (int i = 0; i < newEvents; i++)
            {
                _collection.Append(new TestEvent());
            }

            _collection.ExpectedVersion.Should().Be(_version + newEvents);
        }
    }

    public class CreatedOn
    {
        [Test]
        public void should_be_date_of_first_event_applied_event()
        {
            var collection = EventCollection.Create(Guid.NewGuid());
            var firstEvent = new TestEvent() { OccouredOn = DateTimeOffset.UtcNow };

            collection.Append(firstEvent);

            for (int i = 0; i < rnd.Next(50); i++)
            {
                collection.Append(new TestEvent() {  OccouredOn = DateTimeOffset.UtcNow.AddMinutes(i)});
            }

            collection.CreatedOn.Should().Be(firstEvent.OccouredOn);
        }

        [Test]
        public void should_be_date_of_first_event()
        {
            var numEvents = rnd.Next(50);
            var firstEvent = new TestEvent() { OccouredOn = DateTimeOffset.UtcNow };
            var events = new TestEvent[numEvents + 1];

            events[0] = firstEvent;
            for (int i = 1; i < numEvents; i++)
            {
                events[i] = new TestEvent() { OccouredOn = DateTimeOffset.UtcNow.AddMinutes(i) };
            }

            var collection = EventCollection.Create(Guid.NewGuid(), events);

            collection.CreatedOn.Should().Be(firstEvent.OccouredOn);
        }
    }
}