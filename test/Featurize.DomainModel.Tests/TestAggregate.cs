namespace Featurize.DomainModel.Tests;

public record TestEvent() : EventRecord;
public record TestEventWithoutApply() : EventRecord;
public sealed class TestAggregate : AggregateRoot<Guid>
{
    public bool ApplyCalled { get; private set; }
    public int ApplyCalledTimes { get; private set; }

    private TestAggregate(Guid id) : base(id)
    {

    }

    public static TestAggregate Create(Guid id)
    {
        var aggregate = new TestAggregate(id);
        aggregate.RecordEvent(new TestEvent());
        return aggregate;
    }

    internal void Apply(TestEvent e)
    {
        ApplyCalled = true;
        ApplyCalledTimes++;
    }
}

public sealed class CreateTestAggregate : AggregateRoot<Guid>
{
    private CreateTestAggregate() : base(Guid.NewGuid())
    {
    }
}

public sealed class CreateTestAggregateWithPrivateConstructor : AggregateRoot<Guid>
{
    private CreateTestAggregateWithPrivateConstructor(Guid id) : base(id)
    {
    }

    public static CreateTestAggregateWithPrivateConstructor Create()
    {
        return new CreateTestAggregateWithPrivateConstructor(Guid.NewGuid());
    }
}

public sealed class CreateTestAggregateWithPublicConstructor : AggregateRoot<Guid>
{
    public CreateTestAggregateWithPublicConstructor(Guid id) : base(id)
    {
    }
}