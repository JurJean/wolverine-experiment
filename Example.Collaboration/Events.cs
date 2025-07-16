namespace Example.Collaboration;

public static class Events
{
    public sealed record RoomOpened
    {
        public required RoomId RoomId { get; init; }
        
        public required string Name { get; init; }
    }

    public sealed record RoomClosed
    {
        public required RoomId RoomId { get; init; }
        
        public required string Reason { get; init; }
    }
}