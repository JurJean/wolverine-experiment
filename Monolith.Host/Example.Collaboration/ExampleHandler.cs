using Wolverine.Attributes;
using Wolverine.Marten;

namespace Monolith.Host.Example.Collaboration;

public sealed class ExampleHandler
{
    public void Handle(Events.RoomOpened e)
    {
        Console.WriteLine("Room has been opened!!!");
    }
}