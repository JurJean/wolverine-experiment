using System;

namespace Example;

public sealed partial class SimpleAggregate
{
    public Guid Id { get; init; }

    public SimpleAggregate Create(Events.SomethingWasScheduled @event)
    {
        return new SimpleAggregate
        {
            Id = @event.Id,
        };
    }
}