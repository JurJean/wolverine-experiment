using System;
using Wolverine;
using Wolverine.Marten;

namespace Example;

public sealed class SimpleHandler
{
    public DeliveryMessage<Events.SomethingWasScheduled> Handle(
        Commands.ScheduleSomething command,
        [ReadAggregate(Required = false)] SimpleAggregate? aggregate)
    {
        Console.WriteLine("Scheduling event...");
        return new Events.SomethingWasScheduled { Id = command.Id }
            .DelayedFor(TimeSpan.FromSeconds(30));
    }

    public void Handle(Events.SomethingWasScheduled @event)
    {
        Console.WriteLine("Received scheduled event!");
    }
}