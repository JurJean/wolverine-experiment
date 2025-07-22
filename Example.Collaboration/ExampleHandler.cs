using Wolverine.Attributes;

namespace Example.Collaboration;

public sealed class ExampleHandler
{
    [WolverineHandler]
    public void Handle(Events.RoomOpened e)
    {
        Console.WriteLine("Room has been opened!!!");
    }
}