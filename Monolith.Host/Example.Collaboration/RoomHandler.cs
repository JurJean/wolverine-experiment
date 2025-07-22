using Wolverine.Marten;

namespace Monolith.Host.Example.Collaboration;

using static Commands;
using static Events;

public sealed class RoomHandler
{
    [AggregateHandler]
    public IEnumerable<object> Handle(OpenRoom command, Room room)
    {
        yield return new RoomOpened
        {
            RoomId = command.RoomId,
            Name = command.Name,
        };
    }

    [AggregateHandler]
    public IEnumerable<object> Handle(CloseRoom command)
    {
        yield return new RoomClosed
        {
            RoomId = command.RoomId,
            Reason = command.Reason,
        };
    }
}