using Wolverine;
using Wolverine.Kafka;

namespace Example.Collaboration;

public class CollaborationExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(typeof(CollaborationExtension).Assembly);
        options.Discovery.IncludeType<ExampleHandler>();
        
        Console.WriteLine("-------------");
        Console.WriteLine(options.DescribeHandlerMatch(typeof(ExampleHandler)));
        Console.WriteLine("-------------");
        Console.WriteLine(options.DescribeHandlerMatch(typeof(RoomHandler)));
        Console.WriteLine("-------------");
        
        options.PublishMessage<Events.RoomOpened>()
            .ToKafkaTopic("room.opened");
        options.PublishMessage<Events.RoomClosed>()
            .ToKafkaTopic("room.closed");
    }
}