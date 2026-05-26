using System;
using System.Threading.Tasks;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Marten;

namespace Example;

[WolverineHandler]
public sealed class SimpleHandler
{
    public async Task Handle(
        Commands.ScheduleSomething command,
        IMessageContext context,
        [ReadAggregate(Required = false)] SimpleAggregate? aggregate)
    {
        Console.WriteLine("Scheduling something...");

        await context.ScheduleAsync(
            new Events.SomethingWasScheduled { Id = command.Id },
            TimeSpan.FromSeconds(30));
    }
    
    public void Handle(Events.SomethingWasScheduled command)
    {
        Console.WriteLine("Received scheduled event!");
    }
}