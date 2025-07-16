using Wolverine;
using Wolverine.Kafka;

namespace Example.Collaboration;

public class CollaborationExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(typeof(CollaborationExtension).Assembly);
        
        options.PublishMessage<Events.RoomOpened>()
            .ToKafkaTopic("room.opened");
        options.PublishMessage<Events.RoomOpened>()
            .ToKafkaTopic("room.closed");
    }
}