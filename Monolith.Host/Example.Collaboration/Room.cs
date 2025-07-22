namespace Monolith.Host.Example.Collaboration;

using static Events;

public class Room
{
    public string Id { get; set; }
    
    public bool Open { get; private set; }

    public void Apply(RoomOpened @event)
    {
        Id = @event.RoomId;
        Open = true;
    }

    public void Apply(RoomClosed @event)
    {
        Id = @event.RoomId;
        Open = false;
    }
}