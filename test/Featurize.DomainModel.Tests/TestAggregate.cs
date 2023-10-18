namespace Featurize.DomainModel.Tests;

public record TestEvent() : EventRecord;
public record TestEventWithoutApply() : EventRecord;
public sealed class TestAggregate : AggregateRoot<TestAggregate, Guid>
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

public sealed class CreateTestAggregate : AggregateRoot<CreateTestAggregate, Guid>
{
    public CreateTestAggregate(Guid id) : base(id)
    {
    }
}

public sealed class CreateTestAggregateWithPrivateConstructor : AggregateRoot<CreateTestAggregateWithPrivateConstructor, Guid>
{
    private CreateTestAggregateWithPrivateConstructor(Guid id) : base(id)
    {
    }

    public static CreateTestAggregateWithPrivateConstructor Create()
    {
        return new CreateTestAggregateWithPrivateConstructor(Guid.NewGuid());
    }
}

public sealed class CreateTestAggregateWithPublicConstructor : AggregateRoot<CreateTestAggregateWithPublicConstructor, Guid>
{
    public CreateTestAggregateWithPublicConstructor(Guid id) : base(id)
    {
    }

    public static CreateTestAggregateWithPublicConstructor Create()
    {
        return new CreateTestAggregateWithPublicConstructor(Guid.NewGuid());
    }
}