using System;
using System.Threading.Tasks;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace Example;

[WolverineHandler]
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